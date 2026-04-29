using Dapper;
using RockoCloud.DataAccess.Interfaces;
using RockoCloud.Models;

namespace RockoCloud.DataAccess;

public class SongRepository : ISongRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SongRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Song>> GetAllAsync()
    {
        using var db = _connectionFactory.CreateConnection();
        return await db.QueryAsync<Song>("SELECT * FROM Songs ORDER BY Artist, Title");
    }

    public async Task<int> UpsertAsync(Song song)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
        INSERT INTO Songs (Id, TenantId, Title, Artist, Album, DurationSeconds, LocalPath, Genre, FileName, DateAdded, CoverPath)
        VALUES (@Id, @TenantId, @Title, @Artist, @Album, @DurationSeconds, @LocalPath, @Genre, @FileName, @DateAdded, @CoverPath)
        ON CONFLICT(LocalPath) DO UPDATE SET
            TenantId = excluded.TenantId,
            Title = excluded.Title,
            Artist = excluded.Artist,
            Album = excluded.Album,
            Genre = excluded.Genre,
            CoverPath = excluded.CoverPath;";

        return await db.ExecuteAsync(sql, song);
    }

    public async Task<int> ClearAllAsync()
    {
        using var db = _connectionFactory.CreateConnection();
        return await db.ExecuteAsync("DELETE FROM Songs");
    }
    public async Task<Song?> GetByIdAsync(string id)
    {
        using var db = _connectionFactory.CreateConnection();
        return await db.QueryFirstOrDefaultAsync<Song>("SELECT * FROM Songs WHERE Id = @Id", new { Id = id });
    }

    public async Task<bool> DeleteAsync(string id)
    {
        using var db = _connectionFactory.CreateConnection();
        var rows = await db.ExecuteAsync("DELETE FROM Songs WHERE Id = @Id", new { Id = id });
        return rows > 0;
    }
}