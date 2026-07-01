namespace Application.Common.Interfaces;

public interface IFileService
{
    Task<string> SaveFileAsync(byte[] fileData, string fileName, string folder);
    Task<bool> DeleteFileAsync(string filePath);
    string GetFileUrl(string relativePath);
}
