using MediatR;

using NetArchTest.Rules;

using Xunit;

namespace SynergyFlow.ArchitectureTests.NamingConventions;

public class HandlerNamingTests
{
    [Fact]
    public void RequestHandlers_ShouldHaveNameEndingWithCommandHandlerOrQueryHandler()
    {
        var result = Types.InAssembly(typeof(Application.AssemblyMarker).Assembly)
            .That()
            .ImplementInterface(typeof(IRequestHandler<,>))
            .Should()
            .HaveNameEndingWith("CommandHandler")
            .Or()
            .HaveNameEndingWith("QueryHandler")
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            $"Handlers not following naming convention: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
