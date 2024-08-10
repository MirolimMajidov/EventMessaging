using EventStore.Models.Outbox.Providers;
using UsersService.Messaging.Events.Publishers;
using UsersService.Services;

namespace UsersService.Messaging.Publishers;

public class DeletedUserPublisher : IPublishWebHookEvent<UserDeleted>
{
    private readonly IWebHookProvider _webHookProvider;

    public DeletedUserPublisher(IWebHookProvider webHookProvider)
    {
        _webHookProvider = webHookProvider;
    }
    
    public Task Publish(UserDeleted @event, string eventPath)
    {
        throw new NotImplementedException();
    }
}