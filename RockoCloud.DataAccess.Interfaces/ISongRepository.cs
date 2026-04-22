using RockoCloud.Models;

namespace RockoCloud.DataAccess.Interfaces;

public interface ISongRepository
{
    Task<IEnumerable<Song>> GetAllAsync();
    Task<int> UpsertAsync(Song song);
    Task<int> ClearAllAsync();
}