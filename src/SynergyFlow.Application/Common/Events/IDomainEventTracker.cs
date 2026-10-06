using SynergyFlow.Domain.Common;

namespace SynergyFlow.Application.Common.Events;

public interface IDomainEventTracker
{
    void TrackEntity(Entity entity);

    IReadOnlyCollection<DomainEvent> GetAndClearDomainEvents();
}
