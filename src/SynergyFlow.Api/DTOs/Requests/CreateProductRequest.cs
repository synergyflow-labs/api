namespace SynergyFlow.Api.DTOs.Requests;

public sealed record CreateProductRequest(
    string Name,
    string Sku,
    decimal Price,
    int StockQuantity,
    string? Description = null);
