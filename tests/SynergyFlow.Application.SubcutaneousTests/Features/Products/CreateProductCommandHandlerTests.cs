using SynergyFlow.Application.Common.Interfaces;
using SynergyFlow.Application.Features.Products.Commands.CreateProduct;
using SynergyFlow.Application.SubcutaneousTests.Common;
using SynergyFlow.Domain.Entities.Products;
using SynergyFlow.Tests.Common.Security;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace SynergyFlow.Application.SubcutaneousTests.Features.Products;

public class CreateProductCommandHandlerTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Handle_WithValidCommand_PersistsProductAndCreatesOutboxMessage()
    {
        // Arrange
        TestCurrentUser.Set(TestUsers.Admin.User);
        var mediator = factory.CreateMediator();
        var uniqueSku = $"TEST-SKU-{Guid.NewGuid():N}".ToUpperInvariant();
        var command = new CreateProductCommand(
            Name: "Mechanical Gaming Keyboard",
            Sku: uniqueSku,
            Price: 159.99m,
            StockQuantity: 30,
            Description: "RGB Tenkeyless Mechanical Keyboard");

        // Act
        var result = await mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Equal("Mechanical Gaming Keyboard", result.Value.Name);

        // Verify in database directly
        using var scope = factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        var productInDb = await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == result.Value.Id);

        Assert.NotNull(productInDb);
        Assert.Equal(uniqueSku, productInDb.Sku);

        // Verify domain event was captured into OutboxMessages table
        var outboxMessage = await dbContext.OutboxMessages
            .FirstOrDefaultAsync(m => m.Type == "ProductCreatedEvent" && m.Content.Contains(uniqueSku));

        Assert.NotNull(outboxMessage);
        Assert.Null(outboxMessage.ProcessedOnUtc);
    }

    [Fact]
    public async Task Handle_WithDuplicateSku_ReturnsConflictError()
    {
        // Arrange
        TestCurrentUser.Set(TestUsers.Admin.User);
        var mediator = factory.CreateMediator();
        var duplicateSku = $"DUP-SKU-{Guid.NewGuid():N}".ToUpperInvariant();
        var command1 = new CreateProductCommand("Product 1", duplicateSku, 10m, 1);
        var command2 = new CreateProductCommand("Product 2", duplicateSku, 20m, 2);

        // Act
        var result1 = await mediator.Send(command1);
        var result2 = await mediator.Send(command2);

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsFailure);
        Assert.Equal(ProductErrors.SkuAlreadyExists(duplicateSku).Code, result2.TopError.Code);
    }

    [Fact]
    public async Task Handle_WithEmptyName_ShortCircuitsViaValidationBehavior()
    {
        // Arrange
        TestCurrentUser.Set(TestUsers.Admin.User);
        var mediator = factory.CreateMediator();
        var command = new CreateProductCommand(string.Empty, "VALID-SKU", 10m, 1);

        // Act
        var result = await mediator.Send(command);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == nameof(CreateProductCommand.Name));
    }
}
