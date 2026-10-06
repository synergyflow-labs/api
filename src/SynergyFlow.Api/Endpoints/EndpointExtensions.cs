namespace SynergyFlow.Api.Endpoints;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapAllEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapAuthEndpoints();
        app.MapProductEndpoints();

        return app;
    }
}
