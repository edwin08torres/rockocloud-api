using System.Data;

namespace RockoCloud.DataAccess.Interfaces;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}