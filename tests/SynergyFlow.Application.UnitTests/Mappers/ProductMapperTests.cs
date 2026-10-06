using SynergyFlow.Application.Features.Products.Mappers;
using SynergyFlow.Tests.Common.Builders;

using Xunit;

namespace SynergyFlow.Application.UnitTests.Mappers;

public class ProductMapperTests
{
    [Fact]
    public void ToDto_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var product = ProductFactory.CreateValidProduct(
            name: "Mechanical Keyboard",
            sku: "TECH-KB-001",
            price: 129.99m,
            stockQuantity: 20,
            description: "RGB Mechanical Keyboard").Value;

        // Act
        var dto = product.ToDto();

        // Assert
        Assert.Equal(product.Id, dto.Id);
        Assert.Equal(product.Name, dto.Name);
        Assert.Equal(product.Sku, dto.Sku);
        Assert.Equal(product.Price, dto.Price);
        Assert.Equal(product.StockQuantity, dto.StockQuantity);
        Assert.Equal(product.Description, dto.Description);
        Assert.Equal(product.CreatedAtUtc, dto.CreatedAtUtc);
        Assert.Equal(product.UpdatedAtUtc, dto.UpdatedAtUtc);
    }
}
