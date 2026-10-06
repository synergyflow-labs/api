using SynergyFlow.Application.Features.Products.DTOs;
using SynergyFlow.Domain.Entities.Products;

namespace SynergyFlow.Application.Features.Products.Mappers;

public static class ProductMapper
{
    public static ProductDto ToDto(this Product product) =>
        new(
            product.Id,
            product.Name,
            product.Sku,
            product.Price,
            product.StockQuantity,
            product.Description,
            product.CreatedAtUtc,
            product.UpdatedAtUtc);
}
