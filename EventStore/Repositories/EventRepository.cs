using Dapper;
using EventStore.Inbox.Configurations;
using EventStore.Models;
using EventStore.Models.Exceptions;
using Npgsql;

namespace EventStore.Repositories;

internal abstract class EventRepository<TBaseEvent> : IEventRepository<TBaseEvent> where TBaseEvent : IBaseEventBox
{
    private readonly string _tableName;
    private readonly string _connectionString;

    public EventRepository(InboxOrOutboxStructure settings)
    {
        _tableName = settings.TableName;
        _connectionString = settings.ConnectionString;
    }

    public void CreateTableIfNotExists()
    {
        using (var dbConnection = new NpgsqlConnection(_connectionString))
        {
            try
            {
                dbConnection.Open();
                var sql = $@"CREATE TABLE IF NOT EXISTS ""{_tableName}""
                (
                    ""Id"" UUID NOT NULL PRIMARY KEY,
                    ""Provider"" VARCHAR(50) NOT NULL,
                    ""EventName"" VARCHAR(100) NOT NULL,
                    ""EventPath"" VARCHAR(255),
                    ""Payload"" TEXT,
                    ""Headers"" TEXT,
                    ""AdditionalData"" TEXT,
                    ""CreatedAt"" TIMESTAMP(0) NOT NULL,
                    ""TryCount"" SMALLINT DEFAULT '0'::SMALLINT NOT NULL,
                    ""TryAfterAt"" TIMESTAMP(0) NOT NULL,
                    ""ProcessedAt"" TIMESTAMP(0) DEFAULT NULL
                );

                ALTER TABLE public.""{_tableName}""
                    OWNER TO admin;

                CREATE INDEX IF NOT EXISTS idx_for_get_unprocessed_events
                    ON public.""{_tableName}"" (""Provider"", ""ProcessedAt"", ""TryAfterAt"");

                CREATE INDEX IF NOT EXISTS idx_for_delete_processed_events
                    ON public.""{_tableName}"" (""ProcessedAt"");";

                dbConnection.Execute(sql);
            }
            catch (Exception e)
            {
                throw new EventStoreException(e, $"Error while checking/creating {_tableName} table.");
            }
        }
    }

    public void InsertEvent(TBaseEvent @event)
    {
        using (var dbConnection = new NpgsqlConnection(_connectionString))
        {
            try
            {
                dbConnection.Open();
                string sql = $@"
                INSERT INTO ""{_tableName}"" (
                    ""Id"", ""Provider"", ""EventName"", ""EventPath"", ""Payload"", ""Headers"", 
                    ""AdditionalData"", ""CreatedAt"", ""TryCount"", ""TryAfterAt""
                ) VALUES (
                    @Id, @Provider, @EventName, @EventPath, @Payload, @Headers, 
                    @AdditionalData, @CreatedAt, @TryCount, @TryAfterAt
                )";

                dbConnection.Execute(sql, @event);
            }
            catch (Exception e)
            {
                throw new EventStoreException(e,
                    $"Error while inserting a new event to the {_tableName} table with the {@event.Id} id.");
            }
        }
    }

    public async Task<IEnumerable<TBaseEvent>> GetUnprocessedEventsAsync(EventProviderType provider)
    {
        using (var dbConnection = new NpgsqlConnection(_connectionString))
        {
            try
            {
                await dbConnection.OpenAsync();

                string sql = $@"
                SELECT * FROM ""{_tableName}""
                WHERE 
                    ""Provider"" = @Provider 
                    AND ""ProcessedAt"" IS NULL
                    AND ""TryAfterAt"" <= @CurrentTime
                ORDER BY ""CreatedAt"" ASC";

                var unprocessedEvents = await dbConnection.QueryAsync<TBaseEvent>(sql, new 
                { 
                    Provider = provider.ToString(), 
                    CurrentTime = DateTime.Now 
                });

                return unprocessedEvents;
            }
            catch (Exception e)
            {
                throw new EventStoreException(e, $"Error while retrieving unprocessed events from the {_tableName} table.");
            }
        }
    }

    public async Task<bool> UpdateEventAsync(TBaseEvent @event)
    {
        using (var dbConnection = new NpgsqlConnection(_connectionString))
        {
            try
            {
                await dbConnection.OpenAsync();

                string sql = $@"
                UPDATE ""{_tableName}""
                SET 
                    ""TryCount"" = @TryCount,
                    ""TryAfterAt"" = @TryAfterAt,
                    ""ProcessedAt"" = @ProcessedAt
                WHERE ""Id"" = @Id";

                var affectedRows = await dbConnection.ExecuteAsync(sql, @event);
                return affectedRows > 0;
            }
            catch (Exception e)
            {
                throw new EventStoreException(e, $"Error while updating the event in the {_tableName} table with the {@event.Id} id.");
            }
        }
    }

    public async Task<bool> UpdateEventsAsync(IEnumerable<TBaseEvent> events)
    {
        using (var dbConnection = new NpgsqlConnection(_connectionString))
        {
            try
            {
                await dbConnection.OpenAsync();

                string sql = $@"
                UPDATE ""{_tableName}""
                SET 
                    ""TryCount"" = @TryCount,
                    ""TryAfterAt"" = @TryAfterAt,
                    ""ProcessedAt"" = @ProcessedAt
                WHERE ""Id"" = @Id";

                var affectedRows = await dbConnection.ExecuteAsync(sql, events);
                return affectedRows > 0;
            }
            catch (Exception e)
            {
                throw new EventStoreException(e, $"Error while updating events of the {_tableName} table.");
            }
        }
    }

    public async Task<bool> DeleteProcessedEventsAsync(DateTime processedAt)
    {
        using (var dbConnection = new NpgsqlConnection(_connectionString))
        {
            try
            {
                await dbConnection.OpenAsync();

                string sql = $@"
                DELETE FROM ""{_tableName}""
                WHERE ""ProcessedAt"" < @ProcessedAt";

                int deletedRows = await dbConnection.ExecuteAsync(sql, new { ProcessedAt = processedAt });
                return deletedRows > 0;
            }
            catch (Exception e)
            {
                throw new EventStoreException(e, $"Error while deleting processed events from the {_tableName} table.");
            }
        }
    }
}
