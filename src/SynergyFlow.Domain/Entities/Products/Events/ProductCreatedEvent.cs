using SynergyFlow.Domain.Common;

namespace SynergyFlow.Domain.Entities.Products.Events;

public sealed record ProductCreatedEvent(Guid ProductId, string Sku, string Name, decimal Price) : DomainEvent;
