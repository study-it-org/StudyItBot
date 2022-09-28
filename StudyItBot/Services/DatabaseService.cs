using NLog;
using StudyItBot.Models;
using SurrealDB.Driver.Rpc;

namespace StudyItBot.Services;

public class DatabaseService
{
    private readonly DatabaseRpc _db;
    private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    public DatabaseService(DatabaseRpc db)
    {
        _db = db;
    }

    private T? UnwrapSafely<T>(RpcResponse response)
    {
        if (response.TryGetError(out var err))
        {
            _logger.Error(err);
            return default;
        }

        _logger.Debug($"Successfully unwrapped {typeof(T).Name} for {response.Id}");
        return response.UncheckedResult.GetObject<T>();
    }

    public async Task<DbUserData?> CreateNewUserAsync(ulong userId, DbUserData? initialData = null)
    {
        var response = await _db.Create($"user:{userId}", initialData ?? new DbUserData());

        return UnwrapSafely<DbUserData>(response);
    }

    public async Task<DbUserData?> SelectUserAsync(ulong userId)
    {
        var response = await _db.Select($"user:{userId}");

        return UnwrapSafely<DbUserData>(response);
    }

    public async Task<DbUserData?> ChangeUserAsync(ulong userId, DbUserData updatedData)
    {
        var response = await _db.Change($"user:{userId}", updatedData);

        return UnwrapSafely<DbUserData>(response);
    }
}