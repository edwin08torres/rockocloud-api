namespace RockoCloud.Models;

public class DownloadUrlRequest
{
    public string Url { get; set; } = string.Empty;
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public string? Genre { get; set; }
}