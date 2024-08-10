using EventStore.Models;

namespace EventStore.Repositories;

internal interface IEventRepository<TBaseEvent> where TBaseEvent : IBaseEventBox
{
    /// <summary>
    /// Creates the table if it does not exist.
    /// </summary>
    void CreateTableIfNotExists();

    /// <summary>
    /// Inserts a new event into the database.
    /// </summary>
    /// <param name="event">The event to insert.</param>
    void InsertEvent(TBaseEvent @event);

    /// <summary>
    /// Retrieves all unprocessed events based on Provider, and TryAfterAt.
    /// </summary>
    /// <param name="provider">The Provider to filter by.</param>
    /// <returns>A list of unprocessed events that match the criteria.</returns>
    Task<IEnumerable<TBaseEvent>> GetUnprocessedEventsAsync(EventProviderType provider);

    /// <summary>
    /// Updates the specified Event properties.
    /// </summary>
    /// <param name="event">The event to update.</param>
    /// <returns>Returns true if there are any affected rows.</returns>
    Task<bool> UpdateEventAsync(TBaseEvent @event);

    /// <summary>
    /// Updates the specified events' properties.
    /// </summary>
    /// <param name="events">Events to update.</param>
    /// <returns>Returns true if there are any affected rows.</returns>
    Task<bool> UpdateEventsAsync(IEnumerable<TBaseEvent> events);

    /// <summary>
    /// Deletes all processed events which processed before the specified date.
    /// </summary>
    /// <param name="processedAt">The processed date to filter records.</param>
    /// <returns>Returns true if there are any affected rows.</returns>
    Task<bool> DeleteProcessedEventsAsync(DateTime processedAt);
}