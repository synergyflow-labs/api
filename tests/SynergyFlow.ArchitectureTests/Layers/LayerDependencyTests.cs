using NetArchTest.Rules;

using Xunit;

namespace SynergyFlow.ArchitectureTests.Layers;

public class LayerDependencyTests
{
    private const string DomainNamespace = "SynergyFlow.Domain";
    private const string ApplicationNamespace = "SynergyFlow.Application";
    private const string InfrastructureNamespace = "SynergyFlow.Infrastructure";
    private const string ApiNamespace = "SynergyFlow.Api";

    [Fact]
    public void Domain_ShouldNotHaveDependencyOnOtherProjects()
    {
        var otherProjects = new[]
        {
            ApplicationNamespace,
            InfrastructureNamespace,
            ApiNamespace,
        };

        var result = Types.InAssembly(typeof(Domain.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(otherProjects)
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            $"Domain layer has illegal dependencies: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Application_ShouldNotHaveDependencyOnInfrastructureOrApi()
    {
        var forbiddenProjects = new[]
        {
            InfrastructureNamespace,
            ApiNamespace,
        };

        var result = Types.InAssembly(typeof(Application.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenProjects)
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            $"Application layer has illegal dependencies: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Infrastructure_ShouldNotHaveDependencyOnApi()
    {
        var result = Types.InAssembly(typeof(Infrastructure.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            $"Infrastructure layer has illegal dependency on Api: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
