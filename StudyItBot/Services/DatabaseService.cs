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

    #region User Functions

    public async Task<DbUserData?> CreateUserAsync(ulong userId, DbUserData? initialData = null)
    {
        var response = await _db.Create(DbUserData.GetKey(userId), initialData ?? new DbUserData());
        return UnwrapSafely<DbUserData>(response);
    }

    public async Task<DbUserData?> SelectUserAsync(ulong userId)
    {
        var response = await _db.Select(DbUserData.GetKey(userId));
        return UnwrapSafely<DbUserData>(response);
    }

    public async Task<DbUserData?> ChangeUserAsync(ulong userId, DbUserData updatedData)
    {
        var response = await _db.Change(DbUserData.GetKey(userId), updatedData);
        return UnwrapSafely<DbUserData>(response);
    }

    #endregion

    #region Guild Functions

    public async Task<DbGuildData?> CreateGuildSettingsAsync(ulong guildId, DbGuildData? initialData = null)
    {
        var response = await _db.Create(DbGuildData.GetKey(guildId), initialData ?? new DbGuildData());
        return UnwrapSafely<DbGuildData>(response);
    }

    public async Task<DbGuildData?> SelectGuildSettingsAsync(ulong guildId)
    {
        var response = await _db.Select(DbGuildData.GetKey(guildId));
        return UnwrapSafely<DbGuildData>(response);
    }

    public async Task<DbGuildData?> ChangeGuildSettingsAsync(ulong userId, DbGuildData updatedData)
    {
        var response = await _db.Change(DbGuildData.GetKey(userId), updatedData);
        return UnwrapSafely<DbGuildData>(response);
    }

    #endregion
}