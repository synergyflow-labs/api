using SynergyFlow.Application.Common.Behaviours;
using SynergyFlow.Application.Features.Products.Commands.CreateProduct;
using SynergyFlow.Application.Features.Products.DTOs;
using SynergyFlow.Domain.Common.Results;

using MediatR;

using Microsoft.Extensions.Logging;

using NSubstitute;

using Xunit;

namespace SynergyFlow.Application.UnitTests.Behaviours;

public class UnhandledExceptionBehaviourTests
{
    private readonly ILogger<CreateProductCommand> _mockLogger;
    private readonly RequestHandlerDelegate<Result<ProductDto>> _mockNext;
    private readonly UnhandledExceptionBehaviour<CreateProductCommand, Result<ProductDto>> _behavior;

    public UnhandledExceptionBehaviourTests()
    {
        _mockLogger = Substitute.For<ILogger<CreateProductCommand>>();
        _mockNext = Substitute.For<RequestHandlerDelegate<Result<ProductDto>>>();
        _behavior = new UnhandledExceptionBehaviour<CreateProductCommand, Result<ProductDto>>(_mockLogger);
    }

    [Fact]
    public async Task Handle_WhenNoException_ReturnsResponse()
    {
        // Arrange
        var command = new CreateProductCommand("Name", "SKU", 10m, 1);
        var expectedResponse = (Result<ProductDto>)new ProductDto(
            Guid.NewGuid(),
            "Name",
            "SKU",
            10m,
            1,
            null,
            DateTime.UtcNow,
            null);

        _mockNext.Invoke(Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _behavior.Handle(command, _mockNext, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedResponse.Value, result.Value);
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_LogsAndRethrows()
    {
        // Arrange
        var command = new CreateProductCommand("Name", "SKU", 10m, 1);
        var exception = new InvalidOperationException("Something bad happened");

        _mockNext.Invoke(Arg.Any<CancellationToken>()).Returns<Result<ProductDto>>(_ => throw exception);

        // Act & Assert
        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _behavior.Handle(command, _mockNext, CancellationToken.None));

        Assert.Same(exception, thrown);
    }
}
