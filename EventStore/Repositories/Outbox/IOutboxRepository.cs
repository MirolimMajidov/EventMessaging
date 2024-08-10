using EventStore.Models.Outbox;

namespace EventStore.Repositories.Outbox;

internal interface IOutboxRepository: IEventRepository<OutboxEvent>
{
}