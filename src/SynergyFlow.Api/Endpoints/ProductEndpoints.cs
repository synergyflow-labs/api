using SynergyFlow.Api.DTOs.Requests;
using SynergyFlow.Application.Common.Models;
using SynergyFlow.Application.Features.Products.Commands.CreateProduct;
using SynergyFlow.Application.Features.Products.DTOs;
using SynergyFlow.Application.Features.Products.Queries.GetProductById;
using SynergyFlow.Application.Features.Products.Queries.GetProducts;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace SynergyFlow.Api.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/products")
            .WithTags("Products");

        group.MapPost(string.Empty, async (
            [FromBody] CreateProductRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new CreateProductCommand(
                request.Name,
                request.Sku,
                request.Price,
                request.StockQuantity,
                request.Description);

            var result = await sender.Send(command, ct);
            return result.Match(
                p => Results.Created($"api/v1/products/{p.Id}", p),
                ResultExtensions.Problem);
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .Produces<ProductDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("{id:guid}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetProductByIdQuery(id);
            var result = await sender.Send(query, ct);
            return result.ToOk();
        })
        .AllowAnonymous()
        .Produces<ProductDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet(string.Empty, async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? search,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetProductsQuery(page <= 0 ? 1 : page, pageSize <= 0 ? 10 : pageSize, search);
            var result = await sender.Send(query, ct);
            return result.ToOk();
        })
        .AllowAnonymous()
        .Produces<PaginatedList<ProductDto>>(StatusCodes.Status200OK);

        return app;
    }
}
