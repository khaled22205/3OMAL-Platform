using src.Services.Interfaces;

namespace src.Services.Implementations;

public class FileService : IFileService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public FileService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public async Task<string> SaveFileAsync(byte[] fileData, string fileName, string folder)
    {
        var basePath = _configuration.GetSection("FileStorage").GetValue<string>("BasePath") ?? "Uploads";
        var uploadsDir = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), basePath, folder);
        Directory.CreateDirectory(uploadsDir);

        var uniqueName = $"{Guid.NewGuid():N}_{fileName}";
        var filePath = Path.Combine(uploadsDir, uniqueName);

        await File.WriteAllBytesAsync(filePath, fileData);

        return Path.Combine(basePath, folder, uniqueName).Replace("\\", "/");
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), filePath);
        if (!File.Exists(fullPath)) return Task.FromResult(false);

        File.Delete(fullPath);
        return Task.FromResult(true);
    }

    public string GetFileUrl(string relativePath)
    {
        return $"/{relativePath.Replace("\\", "/")}";
    }
}