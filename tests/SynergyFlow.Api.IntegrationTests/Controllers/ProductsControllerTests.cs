using System.Net;

using SynergyFlow.Api.DTOs.Requests;
using SynergyFlow.Api.IntegrationTests.Common;
using SynergyFlow.Application.Common.Models;
using SynergyFlow.Application.Features.Products.DTOs;
using SynergyFlow.Tests.Common.Security;

using Xunit;

namespace SynergyFlow.Api.IntegrationTests.Controllers;

public class ProductsControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Create_WithoutAuth_Returns401Unauthorized()
    {
        // Arrange
        using var client = factory.CreateAppHttpClient();
        var request = new CreateProductRequest("Keycap Set", "KC-001", 39.99m, 20);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/products", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_AsUser_Returns403Forbidden()
    {
        // Arrange
        using var client = factory.CreateAppHttpClient();
        await client.AuthenticateAsync(TestUsers.User.User.Email!, TestUsers.User.Password);
        var request = new CreateProductRequest("Keycap Set", "KC-002", 39.99m, 20);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/products", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_AsAdmin_Returns201Created()
    {
        // Arrange
        using var client = factory.CreateAppHttpClient();
        await client.AuthenticateAsync(TestUsers.Admin.User.Email!, TestUsers.Admin.Password);
        var uniqueSku = $"DESK-MAT-{Guid.NewGuid():N}".ToUpperInvariant();
        var request = new CreateProductRequest("Large Desk Mat", uniqueSku, 29.99m, 50, "Waterproof desk pad");

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/products", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(created);
        Assert.Equal("Large Desk Mat", created.Name);
        Assert.Equal(uniqueSku, created.Sku);
        Assert.Equal(29.99m, created.Price);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_Returns404NotFound()
    {
        // Arrange
        using var client = factory.CreateAppHttpClient();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/v1/products/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPaged_Returns200WithPaginatedProducts()
    {
        // Arrange
        using var client = factory.CreateAppHttpClient();

        // Act
        var response = await client.GetAsync("/api/v1/products?page=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paginatedList = await response.Content.ReadFromJsonAsync<PaginatedList<ProductDto>>();
        Assert.NotNull(paginatedList);
        Assert.True(paginatedList.TotalCount >= 0);
    }
}
