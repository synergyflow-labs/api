using FluentValidation;

using NetArchTest.Rules;

using Xunit;

namespace SynergyFlow.ArchitectureTests.NamingConventions;

public class ValidatorNamingTests
{
    [Fact]
    public void Validators_ShouldHaveNameEndingWithValidator()
    {
        var result = Types.InAssembly(typeof(Application.AssemblyMarker).Assembly)
            .That()
            .Inherit(typeof(AbstractValidator<>))
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            $"Validators not following naming convention: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
