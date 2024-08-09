using EventStore.Models;

namespace EventStore.Repositories;

internal interface IEventRepository: IDisposable
{
    /// <summary>
    /// Creates the table if it does not exist.
    /// </summary>
    void CreateTableIfNotExists();

    /// <summary>
    /// Inserts a new event into the database.
    /// </summary>
    /// <param name="event">The event to insert.</param>
    void InsertEvent(IBaseEventBox @event);

    /// <summary>
    /// Retrieves an event by its Id.
    /// </summary>
    /// <param name="id">The Id of the event.</param>
    /// <returns>The event with the specified Id.</returns>
    Task<IBaseEventBox> GetEventByIdAsync(Guid id);

    /// <summary>
    /// Retrieves all unprocessed events based on Provider, and TryAfterAt.
    /// </summary>
    /// <param name="provider">The Provider to filter by.</param>
    /// <param name="currentTime">The current DateTimeOffset to filter TryAfterAt.</param>
    /// <returns>A list of unprocessed events that match the criteria.</returns>
    Task<IEnumerable<IBaseEventBox>> GetUnprocessedEventsAsync(EventProviderType provider, DateTimeOffset currentTime);

    /// <summary>
    /// Updates the specified Event properties.
    /// </summary>
    /// <param name="event">The event to update.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> UpdateEventAsync(IBaseEventBox @event);

    /// <summary>
    /// Deletes all processed events created before the specified date.
    /// </summary>
    /// <param name="createdAt">The cutoff date to filter records.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> DeleteProcessedEventsAsync(DateTimeOffset createdAt);
}