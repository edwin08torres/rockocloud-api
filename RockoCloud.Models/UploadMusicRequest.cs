using Microsoft.AspNetCore.Http;

namespace RockoCloud.Models;

public class UploadMusicRequest
{
    public IFormFile File { get; set; }
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public string? Genre { get; set; }
}