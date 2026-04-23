using RockoCloud.BusinessLogic.Interfaces;
using RockoCloud.DataAccess.Interfaces;
using RockoCloud.Models;
using System.Diagnostics;

namespace RockoCloud.BusinessLogic;

public class MusicScannerService : IMusicScannerService
{
    private readonly ISongRepository _songRepository;
    private readonly string[] _supportedExtensions = { ".mp3", ".mp4", ".mkv", ".avi" };

    public MusicScannerService(ISongRepository songRepository)
    {
        _songRepository = songRepository;
    }

    public async Task ScanFolderAsync(string path)
    {
        if (!Directory.Exists(path)) return;

        var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
            .Where(f => _supportedExtensions.Contains(Path.GetExtension(f).ToLower()));

        foreach (var file in files)
        {
            try
            {
                using var tfile = TagLib.File.Create(file);

                string? coverLocalPath = null;
                if (tfile.Tag.Pictures.Length > 0)
                {
                    var pic = tfile.Tag.Pictures[0];
                    coverLocalPath = Path.Combine(Path.GetDirectoryName(file)!, "cover.jpg");
                    // Solo la guardamos si no existe ya
                    if (!File.Exists(coverLocalPath))
                    {
                        File.WriteAllBytes(coverLocalPath, pic.Data.Data);
                    }
                }

                var song = new Song
                {
                    Title = tfile.Tag.Title ?? Path.GetFileNameWithoutExtension(file),
                    Artist = tfile.Tag.FirstPerformer ?? "Artista Desconocido",
                    Album = tfile.Tag.Album ?? "Album Desconocido",
                    DurationSeconds = (int)tfile.Properties.Duration.TotalSeconds,
                    CoverPath = coverLocalPath,
                    LocalPath = file,
                    Genre = tfile.Tag.FirstGenre ?? "General",
                    FileName = Path.GetFileName(file)
                };

                await _songRepository.UpsertAsync(song);
            }
            catch (Exception) { /* Log error scanning specific file */ }
        }
    }
}