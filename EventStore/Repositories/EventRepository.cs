using System.Data;
using Dapper;
using EventStore.Inbox.Configurations;
using EventStore.Models;
using EventStore.Models.Exceptions;
using EventStore.Models.Outbox;
using Npgsql;

namespace EventStore.Repositories;

internal abstract class EventRepository<TBaseEvent> : IEventRepository<TBaseEvent> where TBaseEvent : IBaseEventBox
{
    private readonly string _tableName;

    private readonly IDbConnection _dbConnection;

    public EventRepository(InboxOrOutboxStructure settings)
    {
        _tableName = settings.TableName;
        _dbConnection = new NpgsqlConnection(settings.ConnectionString);
    }

    public void CreateTableIfNotExists()
    {
        try
        {
            _dbConnection.Open();
            var sql = $@"create table if not exists ""{_tableName}""
                (
                    ""Id"" UUID NOT NULL PRIMARY KEY,
                    ""Provider"" VARCHAR(50) NOT NULL,
                    ""EventName"" VARCHAR(100) NOT NULL,
                    ""EventPath"" VARCHAR(255),
                    ""Payload"" TEXT,
                    ""Headers"" TEXT,
                    ""AdditionalData"" TEXT,
                    ""CreatedAt"" timestamp(0) NOT NULL,
                    ""TryCount"" smallint default '0'::smallint not null,
                    ""TryAfterAt"" timestamp(0) NOT NULL,
                    ""ProcessedAt"" timestamp(0),
                    ""Processed"" BOOLEAN NOT NULL
                );

                alter table public.""{_tableName}""
                    owner to admin;

                create index if not exists idx_processed_createdat
                    on public.""{_tableName}"" (""Processed"", ""CreatedAt"");

                create index if not exists idx_unprocessed_provider_eventtype
                    on public.""{_tableName}"" (""Processed"", ""CreatedAt"", ""TryAfterAt"");";

            _dbConnection.Execute(sql);
        }
        catch (Exception e)
        {
            throw new EventStoreException(e, $"Error while checking/creating {_tableName} table.");
        }
        finally
        {
            _dbConnection.Close();
        }
    }

    public void InsertEvent(TBaseEvent @event)
    {
        try
        {
            _dbConnection.Open();

            string sql = $@"
            INSERT INTO ""{_tableName}"" (
                ""Id"", ""Provider"", ""EventName"", ""EventPath"", ""Payload"", ""Headers"", 
                ""AdditionalData"", ""CreatedAt"", ""TryCount"", ""TryAfterAt"", ""ProcessedAt"", ""Processed""
            ) VALUES (
                @Id, @Provider, @EventName, @EventPath, @Payload, @Headers, 
                @AdditionalData, @CreatedAt, @TryCount, @TryAfterAt, @ProcessedAt, @Processed
            )";

            _dbConnection.Execute(sql, @event);
        }
        catch (Exception e)
        {
            throw new EventStoreException(e,
                $"Error while inserting a new event to the {_tableName} table with the {@event.Id} id.");
        }
        finally
        {
            _dbConnection.Close();
        }
    }

    public async Task<IEnumerable<TBaseEvent>> GetUnprocessedEventsAsync(EventProviderType provider,
        DateTime currentTime)
    {
        try
        {
            _dbConnection.Open();

            string sql = $@"
            SELECT * FROM ""{_tableName}""
            WHERE 
                ""Provider"" = @Provider 
                AND ""Processed"" = false
                AND ""TryAfterAt"" <= @CurrentTime
            ORDER BY ""CreatedAt"" ASC";

            var unprocessedEvents = await _dbConnection.QueryAsync<TBaseEvent>(sql, new 
            { 
                Provider = provider.ToString(), 
                CurrentTime = currentTime 
            });
            
            return unprocessedEvents;
        }
        catch (Exception e)
        {
            throw new EventStoreException(e, $"Error while retrieving unprocessed events from the {_tableName} table.");
        }
        finally
        {
            _dbConnection.Close();
        }
    }

    public async Task<bool> UpdateEventAsync(TBaseEvent @event)
    {
        try
        {
            _dbConnection.Open();

            string sql = $@"
            UPDATE ""{_tableName}""
            SET 
                ""TryCount"" = @TryCount,
                ""TryAfterAt"" = @TryAfterAt,,
                ""ProcessedAt"" = @ProcessedAt
                ""Processed"" = @Processed
            WHERE ""Id"" = @Id";

            var affectedRows = await _dbConnection.ExecuteAsync(sql, @event);
            return affectedRows > 0;
        }
        catch (Exception e)
        {
            throw new EventStoreException(e, $"Error while updating the event in the {_tableName} table with the {@event.Id} id.");
        }
        finally
        {
            _dbConnection.Close();
        }
    }

    public async Task<bool> UpdateEventsAsync(TBaseEvent[] events)
    {
        try
        {
            _dbConnection.Open();

            string sql = $@"
            UPDATE ""{_tableName}""
            SET 
                ""TryCount"" = @TryCount,
                ""TryAfterAt"" = @TryAfterAt,
                ""ProcessedAt"" = @ProcessedAt,
                ""Processed"" = @Processed
            WHERE ""Id"" = @Id";

            var affectedRows = await _dbConnection.ExecuteAsync(sql, events);
            return affectedRows > 0;
        }
        catch (Exception e)
        {
            throw new EventStoreException(e, $"Error while updating events of the {_tableName} table.");
        }
        finally
        {
            _dbConnection.Close();
        }
    }

    public async Task<bool> DeleteProcessedEventsAsync(DateTime createdAt)
    {
        try
        {
            _dbConnection.Open();

            string sql = $@"
            DELETE FROM ""{_tableName}""
            WHERE ""Processed"" = true 
            AND ""CreatedAt"" < @CreatedAt";

            int deletedRows = await _dbConnection.ExecuteAsync(sql, new { CreatedAt = createdAt });
            return deletedRows > 0;
        }
        catch (Exception e)
        {
            throw new EventStoreException(e, $"Error while deleting processed events from the {_tableName} table.");
        }
        finally
        {
            _dbConnection.Close();
        }
    }


    #region Dispose

    private bool _disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
                _dbConnection?.Dispose();
            _disposed = true;
        }
    }

    ~EventRepository()
    {
        Dispose(false);
    }

    #endregion
}