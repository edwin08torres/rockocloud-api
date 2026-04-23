namespace RockoCloud.BusinessLogic.Interfaces;

public interface IDownloadService
{
    Task<string> DownloadFromYouTubeAsync(string url, string destinationPath);
}