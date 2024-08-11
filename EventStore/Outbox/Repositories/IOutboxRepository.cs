using EventStore.Outbox.Models;
using EventStore.Repositories;

namespace EventStore.Outbox.Repositories;

internal interface IOutboxRepository: IEventRepository<OutboxEvent>
{
}