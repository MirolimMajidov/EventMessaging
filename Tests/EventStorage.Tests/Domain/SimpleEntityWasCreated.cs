using EventStorage.Inbox.Models;

namespace EventStorage.Tests.Domain;

public class SimpleEntityWasCreated : IReceiveEvent
{
    public Guid Id { get; set; }

    public string Type { get; set; }
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; }
}