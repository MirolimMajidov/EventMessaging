using EventStorage.Inbox.Providers;

namespace EventStorage.Tests.Domain;

public class SimpleEntityWasCreatedHandler: IEventReceiver<SimpleEntityWasCreated>
{
    public Task<bool> Receive(SimpleEntityWasCreated @event)
    {
        return Task.FromResult(true);
    }
}