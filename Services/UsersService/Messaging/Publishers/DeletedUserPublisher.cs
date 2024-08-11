using EventStore.Outbox.Providers;
using UsersService.Messaging.Events.Publishers;
using UsersService.Services;

namespace UsersService.Messaging.Publishers;

public class DeletedUserPublisher : IWebHookEventSender<UserDeleted>
{
    private readonly IWebHookProvider _webHookProvider;

    public DeletedUserPublisher(IWebHookProvider webHookProvider)
    {
        _webHookProvider = webHookProvider;
    }

    public async Task<bool> Publish(UserDeleted @event, string eventPath)
    {
        //Add your logic
        return await Task.FromResult(false);
    }
}