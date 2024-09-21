using EventStorage.Outbox.Providers.EventProviders;

namespace EventStorage.Tests.Domain;

public class SimpleSendEventCreatedHandler: IMessageBrokerEventPublisher<SimpleSendEventCreated>
{
    public Task<bool> Publish(SimpleSendEventCreated @event, string eventPath)
    {
        return Task.FromResult(true);
    }
}