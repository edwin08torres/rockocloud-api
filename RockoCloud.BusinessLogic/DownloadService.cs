using RockoCloud.BusinessLogic.Interfaces;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace RockoCloud.BusinessLogic;

public class DownloadService : IDownloadService
{
    private readonly YoutubeClient _youtube;

    public DownloadService()
    {
        _youtube = new YoutubeClient();
    }

    public async Task<string> DownloadFromYouTubeAsync(string url, string destinationPath)
    {
        var video = await _youtube.Videos.GetAsync(url);

        string cleanTitle = string.Join("_", video.Title.Split(Path.GetInvalidFileNameChars()));

        var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(url);

        var streamInfo = streamManifest.GetMuxedStreams()
            .Where(s => s.Container == Container.Mp4)
            .GetWithHighestVideoQuality();

        if (streamInfo is null)
        {
            streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();

            if (streamInfo is null)
                throw new Exception("No se encontró un formato de video con audio integrado válido.");
        }

        var directory = Path.GetDirectoryName(destinationPath);
        var finalPath = Path.Combine(directory!, $"{cleanTitle}.{streamInfo.Container.Name}");

        await _youtube.Videos.Streams.DownloadAsync(streamInfo, finalPath);

        return finalPath;
    }
}