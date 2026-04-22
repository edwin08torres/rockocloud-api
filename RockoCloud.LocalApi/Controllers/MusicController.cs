using Microsoft.AspNetCore.Mvc;
using RockoCloud.BusinessLogic.Interfaces;
using RockoCloud.DataAccess.Interfaces;

namespace RockoCloud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MusicController : ControllerBase
{
    private readonly ISongRepository _songRepository;
    private readonly IMusicScannerService _scannerService;

    public MusicController(ISongRepository songRepository, IMusicScannerService scannerService)
    {
        _songRepository = songRepository;
        _scannerService = scannerService;
    }

    [HttpGet("library")]
    public async Task<IActionResult> GetLibrary()
    {
        var songs = await _songRepository.GetAllAsync();
        return Ok(songs);
    }

    [HttpPost("scan")]
    public async Task<IActionResult> StartScan([FromBody] string path)
    {
        await _scannerService.ScanFolderAsync(path);
        return Ok(new { message = "Escaneo completado" });
    }
}