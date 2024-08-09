using System.Data;
using Dapper;
using EventStore.Inbox.Configurations;
using EventStore.Models;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace EventStore.Repositories;

internal abstract class EventRepository : IEventRepository
{
    private readonly ILogger _logger;
    private readonly string _tableName;

    private readonly IDbConnection _dbConnection;

    public EventRepository(InboxOrOutboxStructure settings, ILogger logger)
    {
        _logger = logger;
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
                    ""Provider"" INTEGER NOT NULL,
                    ""EventPath"" VARCHAR(255),
                    ""Payload"" TEXT,
                    ""Metadata"" TEXT,
                    ""CreatedAt"" TIMESTAMPTZ NOT NULL,
                    ""TryCount"" INTEGER NOT NULL,
                    ""TryAfterAt"" TIMESTAMPTZ NOT NULL,
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
            _logger.LogError(e, "Error while checking/creating {TableName} table.", _tableName);
            throw;
        }
        finally
        {
            _dbConnection.Close();
        }
    }

    public void InsertEvent(IBaseEventBox @event)
    {
        try
        {
            _dbConnection.Open();

            string sql = $@"
            INSERT INTO ""{_tableName}"" (
                ""Id"", ""Provider"", ""EventPath"", ""Payload"", ""Metadata"", 
                ""CreatedAt"", ""TryCount"", ""TryAfterAt"", ""Processed""
            ) VALUES (
                @Id, @Provider, @EventPath, @Payload, @Metadata, 
                @CreatedAt, @TryCount, @TryAfterAt, @Processed
            )";

            _dbConnection.Execute(sql, new
            {
                @TableName = _tableName,
                @Id = @event.Id,
                @Provider = (int)@event.Provider,
                @EventPath = @event.EventPath,
                @Payload = @event.Payload,
                @Metadata = @event.Metadata,
                @CreatedAt = @event.CreatedAt,
                @TryCount = @event.TryCount,
                @TryAfterAt = @event.TryAfterAt,
                @Processed = @event.Processed
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while inserting a new event to the {TableName} table with the {id} id.", _tableName, @event.Id);
            throw;
        }
        finally
        {
            _dbConnection.Close();
        }
    }

    public Task<IBaseEventBox> GetEventByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IBaseEventBox>> GetUnprocessedEventsAsync(EventProviderType provider, DateTimeOffset currentTime)
    {
        throw new NotImplementedException();
    }

    public Task<int> UpdateEventAsync(IBaseEventBox @event)
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteProcessedEventsAsync(DateTimeOffset createdAt)
    {
        throw new NotImplementedException();
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