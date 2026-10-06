using SynergyFlow.Domain.Entities.Products;
using SynergyFlow.Domain.Entities.Products.Events;
using SynergyFlow.Tests.Common.Builders;

using Xunit;

namespace SynergyFlow.Domain.UnitTests.Products;

public class ProductTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Act
        var result = ProductFactory.CreateValidProduct();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Test Keyboard", result.Value.Name);
        Assert.Equal("TEST-KB-001", result.Value.Sku);
        Assert.Equal(99.99m, result.Value.Price);
        Assert.Equal(10, result.Value.StockQuantity);
    }

    [Fact]
    public void Create_WithEmptyName_ReturnsNameRequiredError()
    {
        // Act
        var result = Product.Create(string.Empty, "SKU-001", 50m, 5);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == ProductErrors.NameRequired.Code);
    }

    [Fact]
    public void Create_WithEmptySku_ReturnsSkuRequiredError()
    {
        // Act
        var result = Product.Create("Product Name", "   ", 50m, 5);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == ProductErrors.SkuRequired.Code);
    }

    [Fact]
    public void Create_WithZeroOrNegativePrice_ReturnsPriceError()
    {
        // Act
        var result = Product.Create("Product Name", "SKU-001", 0m, 5);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == ProductErrors.PriceNegativeOrZero.Code);
    }

    [Fact]
    public void Create_WithNegativeStock_ReturnsStockError()
    {
        // Act
        var result = Product.Create("Product Name", "SKU-001", 50m, -1);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == ProductErrors.StockNegative.Code);
    }

    [Fact]
    public void Create_EmitsProductCreatedEvent()
    {
        // Act
        var result = ProductFactory.CreateValidProduct();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.DomainEvents);
        var domainEvent = Assert.IsType<ProductCreatedEvent>(result.Value.DomainEvents.First());
        Assert.Equal(result.Value.Id, domainEvent.ProductId);
        Assert.Equal(result.Value.Sku, domainEvent.Sku);
    }

    [Fact]
    public void UpdateDetails_WithValidData_UpdatesPropertiesAndSetsUpdatedAtUtc()
    {
        // Arrange
        var product = ProductFactory.CreateValidProduct().Value;

        // Act
        var updateResult = product.UpdateDetails("Updated Name", 149.99m, "Updated Description");

        // Assert
        Assert.True(updateResult.IsSuccess);
        Assert.Equal("Updated Name", product.Name);
        Assert.Equal(149.99m, product.Price);
        Assert.Equal("Updated Description", product.Description);
        Assert.NotNull(product.UpdatedAtUtc);
    }

    [Fact]
    public void AdjustStock_WithValidDelta_UpdatesStockQuantity()
    {
        // Arrange
        var product = ProductFactory.CreateValidProduct(stockQuantity: 10).Value;

        // Act
        var adjustResult = product.AdjustStock(5);

        // Assert
        Assert.True(adjustResult.IsSuccess);
        Assert.Equal(15, product.StockQuantity);
    }

    [Fact]
    public void AdjustStock_WithExcessiveNegativeDelta_ReturnsStockNegativeError()
    {
        // Arrange
        var product = ProductFactory.CreateValidProduct(stockQuantity: 5).Value;

        // Act
        var adjustResult = product.AdjustStock(-10);

        // Assert
        Assert.True(adjustResult.IsFailure);
        Assert.Equal(ProductErrors.StockNegative.Code, adjustResult.TopError.Code);
    }
}
