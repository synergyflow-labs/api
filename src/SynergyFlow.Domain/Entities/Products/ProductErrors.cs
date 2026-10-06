using SynergyFlow.Domain.Common.Results;

namespace SynergyFlow.Domain.Entities.Products;

public static class ProductErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Product.NotFound", $"The product with ID '{id}' was not found.");

    public static Error SkuAlreadyExists(string sku) =>
        Error.Conflict("Product.SkuAlreadyExists", $"A product with SKU '{sku}' already exists.");

    public static readonly Error NameRequired =
        Error.Validation("Product.NameRequired", "Product name is required.");

    public static readonly Error SkuRequired =
        Error.Validation("Product.SkuRequired", "Product SKU is required.");

    public static readonly Error PriceNegativeOrZero =
        Error.Validation("Product.PriceNegativeOrZero", "Product price must be greater than zero.");

    public static readonly Error StockNegative =
        Error.Validation("Product.StockNegative", "Product stock quantity cannot be negative.");
}
