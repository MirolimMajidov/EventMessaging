using EventStore.Inbox.Models;
using EventStore.Repositories;

namespace EventStore.Inbox.Repositories;

internal interface IInboxRepository: IEventRepository<InboxEvent>
{
}