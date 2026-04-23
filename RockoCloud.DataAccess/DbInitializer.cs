using Dapper;
using RockoCloud.DataAccess.Interfaces;

namespace RockoCloud.DataAccess;

public static class DbInitializer
{
    public static void Initialize(IDbConnectionFactory factory)
    {
        using var db = factory.CreateConnection();

        db.Execute(@"
            CREATE TABLE IF NOT EXISTS Songs (
                Id TEXT PRIMARY KEY,
                Title TEXT,
                Artist TEXT,
                Album TEXT,
                CoverPath TEXT,
                DurationSeconds INTEGER,
                LocalPath TEXT UNIQUE,
                Genre TEXT,
                FileName TEXT,
                DateAdded DATETIME
            );
            
            CREATE TABLE IF NOT EXISTS SystemSettings (
                Key TEXT PRIMARY KEY,
                Value TEXT
            );");
    }
}