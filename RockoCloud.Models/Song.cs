namespace RockoCloud.Models;

public class Song
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string Album { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public string LocalPath { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
}