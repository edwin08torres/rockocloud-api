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
            INSERT INTO Songs (Id, Title, Artist, Album, DurationSeconds, LocalPath, Genre, FileName, DateAdded)
            VALUES (@Id, @Title, @Artist, @Album, @DurationSeconds, @LocalPath, @Genre, @FileName, @DateAdded)
            ON CONFLICT(LocalPath) DO UPDATE SET
                Title = excluded.Title,
                Artist = excluded.Artist,
                Album = excluded.Album;";

        return await db.ExecuteAsync(sql, song);
    }

    public async Task<int> ClearAllAsync()
    {
        using var db = _connectionFactory.CreateConnection();
        return await db.ExecuteAsync("DELETE FROM Songs");
    }
}