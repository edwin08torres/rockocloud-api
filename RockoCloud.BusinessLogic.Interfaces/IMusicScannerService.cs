namespace RockoCloud.BusinessLogic.Interfaces;

public interface IMusicScannerService
{
    Task ScanFolderAsync(string path);
}