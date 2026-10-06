using SynergyFlow.Domain.Common;

namespace SynergyFlow.Application.Common.Events;

public sealed class DomainEventTracker : IDomainEventTracker
{
    private readonly List<Entity> _trackedEntities = [];

    public void TrackEntity(Entity entity)
    {
        _trackedEntities.Add(entity);
    }

    public IReadOnlyCollection<DomainEvent> GetAndClearDomainEvents()
    {
        var events = _trackedEntities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        foreach (var entity in _trackedEntities)
        {
            entity.ClearDomainEvents();
        }

        _trackedEntities.Clear();
        return events;
    }
}
