using RockoCloud.BusinessLogic.Interfaces;

namespace RockoCloud.BusinessLogic;

public class FileManagerService : IFileManagerService
{
    public string GetSafePath(string root, string artist, string album, string fileName)
    {
        string safeArtist = CleanString(string.IsNullOrWhiteSpace(artist) ? "General" : artist);
        string safeAlbum = CleanString(string.IsNullOrWhiteSpace(album) ? "Varios" : album);
        string safeFileName = CleanString(fileName);

        string directoryPath = Path.Combine(root, safeArtist, safeAlbum);

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        return Path.Combine(directoryPath, safeFileName);
    }

    public string MoveFile(string currentPath, string newRoot, string newArtist, string newAlbum, string fileName)
    {
        if (!File.Exists(currentPath)) return currentPath;

        string newPath = GetSafePath(newRoot, newArtist, newAlbum, fileName);

        if (currentPath != newPath)
        {
            File.Move(currentPath, newPath, overwrite: true);
        }

        return newPath;
    }

    public void DeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private string CleanString(string input)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", input.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).Trim();
    }
}