using SynergyFlow.Domain.Common.Results;
using SynergyFlow.Domain.Entities.Products;

namespace SynergyFlow.Tests.Common.Builders;

public static class ProductFactory
{
    public static Result<Product> CreateValidProduct(
        string name = "Test Keyboard",
        string sku = "TEST-KB-001",
        decimal price = 99.99m,
        int stockQuantity = 10,
        string? description = "A test product")
    {
        return Product.Create(name, sku, price, stockQuantity, description);
    }
}
