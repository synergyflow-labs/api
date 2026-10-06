using SynergyFlow.Api.DTOs.Requests;
using SynergyFlow.Application.Common.Models;
using SynergyFlow.Application.Features.Products.Commands.CreateProduct;
using SynergyFlow.Application.Features.Products.DTOs;
using SynergyFlow.Application.Features.Products.Queries.GetProductById;
using SynergyFlow.Application.Features.Products.Queries.GetProducts;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SynergyFlow.Api.Controllers;

[Route("api/v{version:apiVersion}/products")]
public class ProductsController(ISender sender) : ApiController
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken ct)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Sku,
            request.Price,
            request.StockQuantity,
            request.Description);

        var result = await sender.Send(command, ct);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
        }

        return Problem(result.Errors);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var query = new GetProductByIdQuery(id);
        var result = await sender.Send(query, ct);

        return HandleResult(result);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginatedList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<ProductDto>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var query = new GetProductsQuery(page, pageSize, search);
        var result = await sender.Send(query, ct);

        return HandleResult(result);
    }
}
