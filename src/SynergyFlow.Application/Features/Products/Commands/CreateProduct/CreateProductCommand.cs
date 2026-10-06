using SynergyFlow.Application.Features.Products.DTOs;
using SynergyFlow.Domain.Common.Results;

using MediatR;

namespace SynergyFlow.Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string Sku,
    decimal Price,
    int StockQuantity,
    string? Description = null) : IRequest<Result<ProductDto>>;
