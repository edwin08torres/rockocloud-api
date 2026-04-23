namespace RockoCloud.BusinessLogic.Interfaces;

public interface IFileManagerService
{
    string GetSafePath(string root, string artist, string album, string fileName);
    string MoveFile(string currentPath, string newRoot, string newArtist, string newAlbum, string fileName);
    void DeleteFile(string path);
}