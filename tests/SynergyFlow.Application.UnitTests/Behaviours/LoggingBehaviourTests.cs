using SynergyFlow.Application.Common.Behaviours;
using SynergyFlow.Application.Common.Interfaces;
using SynergyFlow.Application.Features.Products.Commands.CreateProduct;

using Microsoft.Extensions.Logging;

using NSubstitute;

using Xunit;

namespace SynergyFlow.Application.UnitTests.Behaviours;

public class LoggingBehaviourTests
{
    private readonly ILogger<CreateProductCommand> _mockLogger;
    private readonly IUser _mockUser;
    private readonly IAuthService _mockAuthService;
    private readonly LoggingBehaviour<CreateProductCommand> _behaviour;

    public LoggingBehaviourTests()
    {
        _mockLogger = Substitute.For<ILogger<CreateProductCommand>>();
        _mockUser = Substitute.For<IUser>();
        _mockAuthService = Substitute.For<IAuthService>();
        _behaviour = new LoggingBehaviour<CreateProductCommand>(_mockLogger, _mockUser, _mockAuthService);
    }

    [Fact]
    public async Task Process_WhenUserIsAuthenticated_FetchesUserNameAndLogs()
    {
        // Arrange
        var command = new CreateProductCommand("Name", "SKU", 10m, 1);
        _mockUser.Id.Returns("user-123");
        _mockAuthService.GetUserNameAsync("user-123", Arg.Any<CancellationToken>()).Returns("John Doe");

        // Act
        await _behaviour.Process(command, CancellationToken.None);

        // Assert
        await _mockAuthService.Received(1).GetUserNameAsync("user-123", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Process_WhenUserIsAnonymous_DoesNotCallAuthService()
    {
        // Arrange
        var command = new CreateProductCommand("Name", "SKU", 10m, 1);
        _mockUser.Id.Returns((string?)null);

        // Act
        await _behaviour.Process(command, CancellationToken.None);

        // Assert
        await _mockAuthService.DidNotReceive().GetUserNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
