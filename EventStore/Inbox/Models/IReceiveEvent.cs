using EventStore.Models;

namespace EventStore.Inbox.Models;

/// <summary>
/// An interface for determine events to receive
/// </summary>
public interface IReceiveEvent : IEvent
{
}