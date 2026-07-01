using FluentAssertions;
using Infrastructure.FileStorage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Infrastructure.Tests;

public class FileServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly Mock<IWebHostEnvironment> _envMock = new();
    private readonly string _tempDir;

    public FileServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _envMock.Setup(x => x.WebRootPath).Returns(_tempDir);

        var configData = new Dictionary<string, string?>
        {
            { "FileStorage:BasePath", "Uploads" }
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    private FileService CreateService()
    {
        return new FileService(_configuration, _envMock.Object);
    }

    [Fact]
    public async Task SaveFileAsync_Should_create_directory_and_save_file()
    {
        var service = CreateService();
        var fileData = "test content"u8.ToArray();
        var fileName = "test.txt";
        var folder = "documents";

        var result = await service.SaveFileAsync(fileData, fileName, folder);

        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Uploads/documents/");
        result.Should().EndWith("_test.txt");

        var fullPath = Path.Combine(_tempDir, result.Replace("/", "\\"));
        File.Exists(fullPath).Should().BeTrue();
        var savedContent = await File.ReadAllBytesAsync(fullPath);
        savedContent.Should().BeEquivalentTo(fileData);

        Directory.Delete(_tempDir, true);
    }

    [Fact]
    public async Task SaveFileAsync_Should_generate_unique_filename()
    {
        var service = CreateService();
        var fileData = "content"u8.ToArray();

        var result1 = await service.SaveFileAsync(fileData, "test.txt", "docs");
        var result2 = await service.SaveFileAsync(fileData, "test.txt", "docs");

        result1.Should().NotBe(result2);

        Directory.Delete(_tempDir, true);
    }

    [Fact]
    public async Task DeleteFileAsync_Should_return_true_when_file_exists()
    {
        var service = CreateService();
        var fileData = "content"u8.ToArray();
        var savedPath = await service.SaveFileAsync(fileData, "to-delete.txt", "temp");

        var result = await service.DeleteFileAsync(savedPath);

        result.Should().BeTrue();

        var fullPath = Path.Combine(_tempDir, savedPath.Replace("/", "\\"));
        File.Exists(fullPath).Should().BeFalse();

        Directory.Delete(_tempDir, true);
    }

    [Fact]
    public async Task DeleteFileAsync_Should_return_false_when_file_does_not_exist()
    {
        var service = CreateService();

        var result = await service.DeleteFileAsync("nonexistent/file.txt");

        result.Should().BeFalse();

        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public async Task DeleteFileAsync_Should_return_false_for_empty_path()
    {
        var service = CreateService();

        var result = await service.DeleteFileAsync("");

        result.Should().BeFalse();

        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void GetFileUrl_Should_return_relative_path_with_forward_slashes()
    {
        var service = CreateService();

        var result = service.GetFileUrl(@"Uploads\documents\file.txt");

        result.Should().Be("/Uploads/documents/file.txt");
    }

    [Fact]
    public void GetFileUrl_Should_not_change_path_with_forward_slashes()
    {
        var service = CreateService();

        var result = service.GetFileUrl("Uploads/documents/file.txt");

        result.Should().Be("/Uploads/documents/file.txt");
    }

    [Fact]
    public async Task SaveFileAsync_Should_store_empty_file()
    {
        var service = CreateService();
        var fileData = Array.Empty<byte>();

        var result = await service.SaveFileAsync(fileData, "empty", "files");

        result.Should().NotBeNull();

        var fullPath = Path.Combine(_tempDir, result.Replace("/", "\\"));
        new FileInfo(fullPath).Length.Should().Be(0);

        Directory.Delete(_tempDir, true);
    }
}
