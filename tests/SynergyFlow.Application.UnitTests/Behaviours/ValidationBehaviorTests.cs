using SynergyFlow.Application.Common.Behaviours;
using SynergyFlow.Application.Features.Products.Commands.CreateProduct;
using SynergyFlow.Application.Features.Products.DTOs;
using SynergyFlow.Domain.Common.Results;

using FluentValidation;
using FluentValidation.Results;

using MediatR;

using NSubstitute;

using Xunit;

namespace SynergyFlow.Application.UnitTests.Behaviours;

public class ValidationBehaviorTests
{
    private readonly ValidationBehavior<CreateProductCommand, Result<ProductDto>> _validationBehavior;
    private readonly IValidator<CreateProductCommand> _mockValidator;
    private readonly RequestHandlerDelegate<Result<ProductDto>> _mockNextBehavior;

    public ValidationBehaviorTests()
    {
        _mockNextBehavior = Substitute.For<RequestHandlerDelegate<Result<ProductDto>>>();
        _mockValidator = Substitute.For<IValidator<CreateProductCommand>>();
        _validationBehavior = new ValidationBehavior<CreateProductCommand, Result<ProductDto>>(_mockValidator);
    }

    [Fact]
    public async Task Handle_WhenValidationSucceeds_ShouldInvokeNext()
    {
        // Arrange
        var command = new CreateProductCommand("Keyboard", "KB-01", 100m, 5);
        var expectedResponse = (Result<ProductDto>)new ProductDto(
            Guid.NewGuid(),
            "Keyboard",
            "KB-01",
            100m,
            5,
            null,
            DateTime.UtcNow,
            null);

        _mockValidator
            .ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _mockNextBehavior.Invoke(Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _validationBehavior.Handle(command, _mockNextBehavior, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedResponse.Value, result.Value);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ShouldShortCircuitWithErrors()
    {
        // Arrange
        var command = new CreateProductCommand(string.Empty, "KB-01", 100m, 5);
        List<ValidationFailure> failures = [new("Name", "Name is required.")];

        _mockValidator
            .ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        // Act
        var result = await _validationBehavior.Handle(command, _mockNextBehavior, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Equal("Name", result.TopError.Code);
        Assert.Equal("Name is required.", result.TopError.Description);
        await _mockNextBehavior.DidNotReceive().Invoke(Arg.Any<CancellationToken>());
    }
}
