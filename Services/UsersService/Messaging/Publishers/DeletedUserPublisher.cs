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

    public Task<bool> Publish(UserDeleted @event, string eventPath)
    {
        //Add your logic
        throw new NotImplementedException();
    }
}