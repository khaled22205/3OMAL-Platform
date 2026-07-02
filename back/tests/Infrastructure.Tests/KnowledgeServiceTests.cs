using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Application.Features.AiAssistant;
using Infrastructure.Data;
using Infrastructure.Services;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests;

public class KnowledgeServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IEmbeddingService> _embeddingMock = new();
    private readonly Mock<ILogger<KnowledgeService>> _loggerMock = new();

    public KnowledgeServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        SeedData();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private void SeedData()
    {
        _context.Categories.AddRange(
            new Category { Id = 1, Name = "Plumbing", Description = "All plumbing services", IsActive = true },
            new Category { Id = 2, Name = "Electrical", Description = "Electrical repairs and installation", IsActive = true }
        );
        _context.SaveChanges();
    }

    private KnowledgeService CreateService()
    {
        // Return same vector for any input — makes all items equally relevant
        _embeddingMock.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>()))
            .ReturnsAsync(new float[] { 0.1f, 0.2f, 0.3f });
        _embeddingMock.Setup(x => x.ComputeSimilarity(It.IsAny<float[]>(), It.IsAny<float[]>()))
            .Returns(0.5);

        return new KnowledgeService(_context, _embeddingMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task InitializeAsync_Should_load_categories()
    {
        var service = CreateService();
        await service.InitializeAsync();

        var result = await service.RetrieveAsync("plumbing", new KnowledgeContext { Roles = [] }, 10);
        result.Should().Contain(r => r.SourceType == "category" && r.Title == "Plumbing");
    }

    [Fact]
    public async Task InitializeAsync_Should_not_load_inactive_categories()
    {
        _context.Categories.Add(new Category { Id = 3, Name = "Hidden", IsActive = false });
        await _context.SaveChangesAsync();

        var service = CreateService();
        await service.InitializeAsync();

        var result = await service.RetrieveAsync("hidden", new KnowledgeContext { Roles = [] }, 10);
        result.Should().NotContain(r => r.Title == "Hidden");
    }

    [Fact]
    public async Task RetrieveAsync_Should_return_public_items_for_any_role()
    {
        var service = CreateService();
        await service.InitializeAsync();

        var result = await service.RetrieveAsync("test",
            new KnowledgeContext { Roles = ["Guest"] }, 10);
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RetrieveAsync_Should_return_topK_results()
    {
        var service = CreateService();
        await service.InitializeAsync();

        var result = await service.RetrieveAsync("test",
            new KnowledgeContext { Roles = [] }, 1);
        result.Count.Should().BeLessThanOrEqualTo(1);
    }

    [Fact]
    public async Task RetrieveAsync_Should_initialize_automatically_when_empty()
    {
        var service = CreateService();
        var result = await service.RetrieveAsync("plumbing",
            new KnowledgeContext { Roles = [] }, 5);
        result.Should().Contain(r => r.SourceType == "category");
    }

    [Fact]
    public async Task RetrieveAsync_Should_apply_similarity_threshold()
    {
        var service = CreateService();
        _embeddingMock.Setup(x => x.ComputeSimilarity(It.IsAny<float[]>(), It.IsAny<float[]>()))
            .Returns(0.04); // Override: below 0.05 threshold

        var result = await service.RetrieveAsync("nothing",
            new KnowledgeContext { Roles = [] }, 10);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task InitializeAsync_Should_load_workers()
    {
        _context.WorkerProfiles.Add(new WorkerProfile
        {
            Id = 1, UserId = 10, Biography = "Expert plumber", Skills = "plumbing, pipes",
            IsAvailable = true, AverageRating = 4.5, CompletedJobs = 20
        });
        _context.Users.Add(new Microsoft.AspNetCore.Identity.IdentityUser<int>
        {
            Id = 10, UserName = "ahmed_plumber", Email = "ahmed@test.com"
        });
        await _context.SaveChangesAsync();

        var service = CreateService();
        await service.InitializeAsync();

        var result = await service.RetrieveAsync("plumber",
            new KnowledgeContext { Roles = [] }, 10);
        result.Should().Contain(r => r.SourceType == "worker");
    }
}
