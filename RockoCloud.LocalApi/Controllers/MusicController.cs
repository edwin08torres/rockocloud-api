using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RockoCloud.BusinessLogic.Interfaces;
using RockoCloud.DataAccess.Interfaces;
using RockoCloud.Models;

namespace RockoCloud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MusicController : ControllerBase
{
    private readonly ISongRepository _songRepository;
    private readonly IMusicScannerService _scannerService;
    private readonly IFileManagerService _fileManager;
    private readonly IDownloadService _downloadService;
    private readonly string _rootMusicFolder = "C:\\RockoCloud_Music";

    public MusicController(
        ISongRepository songRepository,
        IMusicScannerService scannerService,
        IFileManagerService fileManager,
        IDownloadService downloadService)
    {
        _songRepository = songRepository;
        _scannerService = scannerService;
        _fileManager = fileManager;
        _downloadService = downloadService;

        if (!Directory.Exists(_rootMusicFolder)) Directory.CreateDirectory(_rootMusicFolder);
    }

    private string GetTenantId() => User.FindFirst("TenantId")?.Value ?? "";

    [HttpGet("library")]
    public async Task<IActionResult> GetLibrary()
    {
        var tenantId = GetTenantId();
        var songs = await _songRepository.GetAllAsync();
        return Ok(songs.Where(s => s.TenantId == tenantId));
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadMusic([FromForm] UploadMusicRequest request)
    {
        var tenantId = GetTenantId();

        if (request.File == null || request.File.Length == 0)
            return BadRequest("No se envió ningún archivo.");

        string safePath = _fileManager.GetSafePath(_rootMusicFolder, request.Artist, request.Album, request.File.FileName);

        using (var stream = new FileStream(safePath, FileMode.Create))
        {
            await request.File.CopyToAsync(stream);
        }

        using (var tfile = TagLib.File.Create(safePath))
        {
            tfile.Tag.Performers = new[] { request.Artist ?? "General" };
            tfile.Tag.Album = request.Album ?? "Varios";
            tfile.Tag.Genres = new[] { request.Genre ?? "General" };
            tfile.Save();
        }

        await _scannerService.ScanFolderAsync(Path.GetDirectoryName(safePath)!, tenantId);

        return Ok(new { message = "Archivo subido e indexado correctamente." });
    }

    [HttpPost("download")]
    public async Task<IActionResult> DownloadFromLink([FromBody] DownloadUrlRequest request)
    {
        var tenantId = GetTenantId();

        try
        {
            string safeDirectory = _fileManager.GetSafePath(_rootMusicFolder, request.Artist, request.Album, "temp");
            safeDirectory = Path.GetDirectoryName(safeDirectory)!;

            string finalPath = await _downloadService.DownloadFromYouTubeAsync(request.Url, Path.Combine(safeDirectory, "placeholder.mp4"));

            using (var tfile = TagLib.File.Create(finalPath))
            {
                tfile.Tag.Performers = new[] { request.Artist ?? "General" };
                tfile.Tag.Album = request.Album ?? "Varios";
                tfile.Tag.Genres = new[] { request.Genre ?? "General" };
                tfile.Tag.Title = Path.GetFileNameWithoutExtension(finalPath).Replace("_", " ");
                tfile.Save();
            }

            await _scannerService.ScanFolderAsync(Path.GetDirectoryName(finalPath)!, tenantId);

            return Ok(new { message = "Video descargado e indexado correctamente.", path = finalPath });
        }
        catch (Exception ex)
        {
            return BadRequest($"Error al descargar: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMusic(string id, [FromBody] UpdateMusicRequest request)
    {
        var tenantId = GetTenantId();

        var song = await _songRepository.GetByIdAsync(id);
        if (song == null || song.TenantId != tenantId) return NotFound("Canción no encontrada.");

        string newPath = _fileManager.MoveFile(song.LocalPath, _rootMusicFolder, request.Artist, request.Album, song.FileName);

        using (var tfile = TagLib.File.Create(newPath))
        {
            tfile.Tag.Title = request.Title;
            tfile.Tag.Performers = new[] { request.Artist };
            tfile.Tag.Album = request.Album;
            tfile.Tag.Genres = new[] { request.Genre };
            tfile.Save();
        }

        song.Title = request.Title;
        song.Artist = request.Artist;
        song.Album = request.Album;
        song.Genre = request.Genre;
        song.LocalPath = newPath;

        await _songRepository.UpsertAsync(song);

        return Ok(new { message = "Metadatos y ruta actualizados correctamente." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMusic(string id)
    {
        var tenantId = GetTenantId();

        var song = await _songRepository.GetByIdAsync(id);
        if (song == null || song.TenantId != tenantId) return NotFound();

        _fileManager.DeleteFile(song.LocalPath);
        await _songRepository.DeleteAsync(id);

        return Ok(new { message = "Canción eliminada." });
    }

    [HttpPost("scan")]
    public async Task<IActionResult> StartScan()
    {
        var tenantId = GetTenantId();
        await _scannerService.ScanFolderAsync(_rootMusicFolder, tenantId);

        return Ok(new { message = "Escaneo completado y base de datos actualizada." });
    }

    [HttpGet("genres")]
    public async Task<IActionResult> GetGenres()
    {
        var tenantId = GetTenantId();
        var allSongs = await _songRepository.GetAllAsync();

        var genres = allSongs
            .Where(s => s.TenantId == tenantId && !string.IsNullOrWhiteSpace(s.Genre))
            .Select(s => s.Genre)
            .Distinct()
            .OrderBy(g => g)
            .ToList();

        return Ok(genres);
    }

    [HttpGet("albums")]
    public async Task<IActionResult> GetAlbums()
    {
        var tenantId = GetTenantId();
        var allSongs = await _songRepository.GetAllAsync();

        var albums = allSongs
            .Where(s => s.TenantId == tenantId && !string.IsNullOrWhiteSpace(s.Album))
            .Select(s => s.Album)
            .Distinct()
            .OrderBy(a => a)
            .ToList();

        return Ok(albums);
    }

    [HttpPost("preview-link")]
    [AllowAnonymous]
    public async Task<IActionResult> PreviewYouTubeLink([FromBody] DownloadUrlRequest request)
    {
        try
        {
            var youtube = new YoutubeExplode.YoutubeClient();
            var video = await youtube.Videos.GetAsync(request.Url);

            var titleParts = video.Title.Split('-');
            string artist = video.Author.ChannelTitle.Replace(" - Topic", "").Trim();
            string title = video.Title;

            if (titleParts.Length > 1)
            {
                artist = titleParts[0].Trim();
                title = titleParts[1].Trim();
            }

            return Ok(new
            {
                title = title,
                artist = artist,
                duration = video.Duration?.TotalSeconds ?? 0
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"No se pudo leer el video: {ex.Message}");
        }
    }
}