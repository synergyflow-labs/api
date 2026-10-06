namespace SynergyFlow.Application.Features.Products.DTOs;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Sku,
    decimal Price,
    int StockQuantity,
    string? Description,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
