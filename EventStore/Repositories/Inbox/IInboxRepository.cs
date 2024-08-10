using EventStore.Models.Inbox;

namespace EventStore.Repositories.Inbox;

internal interface IInboxRepository: IEventRepository<InboxEvent>
{
}