using System.Text.Json;

using SynergyFlow.Application.Common.Events;
using SynergyFlow.Application.Common.Interfaces;
using SynergyFlow.Domain.Common;
using SynergyFlow.Domain.Common.Results.Abstractions;

using MediatR;

namespace SynergyFlow.Application.Common.Behaviours;

public class DomainEventsPublishingBehavior<TRequest, TResponse>(
    IDomainEventTracker eventTracker,
    IAppDbContext appDbContext)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var response = await next(ct);

        if (!response.IsSuccess)
        {
            return response;
        }

        var trackedEvents = eventTracker.GetAndClearDomainEvents();
        if (trackedEvents.Count != 0)
        {
            var outboxMessages = trackedEvents.Select(domainEvent => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredOnUtc = DateTime.UtcNow,
                Type = domainEvent.GetType().Name,
                Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
            }).ToList();

            await appDbContext.OutboxMessages.AddRangeAsync(outboxMessages, ct);
            await appDbContext.SaveChangesAsync(ct);
        }

        return response;
    }
}
