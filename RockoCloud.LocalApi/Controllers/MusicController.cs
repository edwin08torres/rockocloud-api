using Microsoft.AspNetCore.Mvc;
using RockoCloud.BusinessLogic.Interfaces;
using RockoCloud.DataAccess.Interfaces;
using RockoCloud.Models;

namespace RockoCloud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MusicController : ControllerBase
{
    private readonly ISongRepository _songRepository;
    private readonly IMusicScannerService _scannerService;
    private readonly IFileManagerService _fileManager;
    private readonly IDownloadService _downloadService;
    private readonly string _rootMusicFolder = "C:\\RockoCloud_Music"; // Idealmente esto viene de SystemSettings o appsettings.json

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

    [HttpGet("library")]
    public async Task<IActionResult> GetLibrary()
    {
        var songs = await _songRepository.GetAllAsync();
        return Ok(songs);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadMusic([FromForm] UploadMusicRequest request)
    {
        if (request.File == null || request.File.Length == 0)
            return BadRequest("No se envió ningún archivo.");

        // 1. Calcular ruta segura (Si no manda artista, se va a General/Varios)
        string safePath = _fileManager.GetSafePath(_rootMusicFolder, request.Artist, request.Album, request.File.FileName);

        // 2. Guardar el archivo físico
        using (var stream = new FileStream(safePath, FileMode.Create))
        {
            await request.File.CopyToAsync(stream);
        }

        // 3. Escribir metadatos reales en el archivo usando TagLib
        using (var tfile = TagLib.File.Create(safePath))
        {
            tfile.Tag.Performers = new[] { request.Artist ?? "General" };
            tfile.Tag.Album = request.Album ?? "Varios";
            tfile.Tag.Genres = new[] { request.Genre ?? "General" };
            tfile.Save();
        }

        // 4. Escanear e insertar en DB
        await _scannerService.ScanFolderAsync(Path.GetDirectoryName(safePath)!);

        return Ok(new { message = "Archivo subido e indexado correctamente." });
    }

    [HttpPost("download")]
    public async Task<IActionResult> DownloadFromLink([FromBody] DownloadUrlRequest request)
    {
        try
        {
            string safeDirectory = _fileManager.GetSafePath(_rootMusicFolder, request.Artist, request.Album, "temp");
            safeDirectory = Path.GetDirectoryName(safeDirectory)!; // Quitamos el "temp"

            string finalPath = await _downloadService.DownloadFromYouTubeAsync(request.Url, Path.Combine(safeDirectory, "placeholder.mp4"));

            using (var tfile = TagLib.File.Create(finalPath))
            {
                tfile.Tag.Performers = new[] { request.Artist ?? "General" };
                tfile.Tag.Album = request.Album ?? "Varios";
                tfile.Tag.Genres = new[] { request.Genre ?? "General" };

                tfile.Tag.Title = Path.GetFileNameWithoutExtension(finalPath).Replace("_", " ");
                tfile.Save();
            }

            await _scannerService.ScanFolderAsync(Path.GetDirectoryName(finalPath)!);

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
        var song = await _songRepository.GetByIdAsync(id);
        if (song == null) return NotFound("Canción no encontrada.");

        // 1. Mover archivo físico a su nueva carpeta si cambió el Artista/Album
        string newPath = _fileManager.MoveFile(song.LocalPath, _rootMusicFolder, request.Artist, request.Album, song.FileName);

        // 2. Actualizar etiquetas ID3 (Metadata física)
        using (var tfile = TagLib.File.Create(newPath))
        {
            tfile.Tag.Title = request.Title;
            tfile.Tag.Performers = new[] { request.Artist };
            tfile.Tag.Album = request.Album;
            tfile.Tag.Genres = new[] { request.Genre };
            tfile.Save();
        }

        // 3. Actualizar la base de datos
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
        var song = await _songRepository.GetByIdAsync(id);
        if (song == null) return NotFound();

        // Borrar físico
        _fileManager.DeleteFile(song.LocalPath);

        // Borrar de DB
        await _songRepository.DeleteAsync(id);

        return Ok(new { message = "Canción eliminada." });
    }

    [HttpPost("scan")]
    public async Task<IActionResult> StartScan()
    {
        // Escanea directamente la carpeta raíz que ya definimos arriba
        await _scannerService.ScanFolderAsync(_rootMusicFolder);
        return Ok(new { message = "Escaneo completado y base de datos actualizada." });
    }
}