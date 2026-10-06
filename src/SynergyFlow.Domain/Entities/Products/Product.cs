using SynergyFlow.Domain.Common;
using SynergyFlow.Domain.Common.Results;
using SynergyFlow.Domain.Entities.Products.Events;

namespace SynergyFlow.Domain.Entities.Products;

public class Product : Entity
{
    private Product()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public string Sku { get; private set; } = string.Empty;

    public decimal Price { get; private set; }

    public int StockQuantity { get; private set; }

    public string? Description { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    public static Result<Product> Create(
        string name,
        string sku,
        decimal price,
        int stockQuantity,
        string? description = null)
    {
        List<Error> errors = [];

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add(ProductErrors.NameRequired);
        }

        if (string.IsNullOrWhiteSpace(sku))
        {
            errors.Add(ProductErrors.SkuRequired);
        }

        if (price <= 0)
        {
            errors.Add(ProductErrors.PriceNegativeOrZero);
        }

        if (stockQuantity < 0)
        {
            errors.Add(ProductErrors.StockNegative);
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Sku = sku.Trim().ToUpperInvariant(),
            Price = price,
            StockQuantity = stockQuantity,
            Description = description?.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
        };

        product.AddDomainEvent(new ProductCreatedEvent(product.Id, product.Sku, product.Name, product.Price));

        return product;
    }

    public Result<Updated> UpdateDetails(string name, decimal price, string? description)
    {
        List<Error> errors = [];

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add(ProductErrors.NameRequired);
        }

        if (price <= 0)
        {
            errors.Add(ProductErrors.PriceNegativeOrZero);
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        Name = name.Trim();
        Price = price;
        Description = description?.Trim();
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Updated;
    }

    public Result<Updated> AdjustStock(int quantityDelta)
    {
        if (StockQuantity + quantityDelta < 0)
        {
            return ProductErrors.StockNegative;
        }

        StockQuantity += quantityDelta;
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Updated;
    }
}
