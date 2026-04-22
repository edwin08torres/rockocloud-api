using Microsoft.Data.Sqlite;
using RockoCloud.DataAccess.Interfaces;
using System.Data;

namespace RockoCloud.DataAccess;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection() => new SqliteConnection(_connectionString);
}