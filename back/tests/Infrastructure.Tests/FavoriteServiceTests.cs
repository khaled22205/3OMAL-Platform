using Infrastructure.Data;
using Infrastructure.Services;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Application.Features.Favorites;

namespace Infrastructure.Tests;

public class FavoriteServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly FavoriteService _service;

    public FavoriteServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _service = new FavoriteService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetUserFavoritesAsync_Should_return_paginated_favorites()
    {
        for (int i = 0; i < 5; i++)
        {
            _context.Favorites.Add(new Favorite { CustomerId = 1 });
        }
        await _context.SaveChangesAsync();

        var result = await _service.GetUserFavoritesAsync(1, 1, 3);

        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(3);
    }

    [Fact]
    public async Task GetUserFavoritesAsync_Should_return_empty_for_other_user()
    {
        _context.Favorites.Add(new Favorite { CustomerId = 1 });
        await _context.SaveChangesAsync();

        var result = await _service.GetUserFavoritesAsync(2, 1, 20);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task AddAsync_Should_throw_when_no_target_specified()
    {
        var request = new AddFavoriteRequest();
        var act = () => _service.AddAsync(1, request);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Specify a worker or service to favorite");
    }

    [Fact]
    public async Task AddAsync_Should_throw_on_duplicate()
    {
        var request = new AddFavoriteRequest { WorkerProfileId = 1 };
        _context.Favorites.Add(new Favorite { CustomerId = 1, WorkerProfileId = 1 });
        await _context.SaveChangesAsync();

        var act = () => _service.AddAsync(1, request);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Already in favorites");
    }

    [Fact]
    public async Task AddAsync_Should_create_favorite_with_worker_profile()
    {
        var request = new AddFavoriteRequest { WorkerProfileId = 42 };

        var result = await _service.AddAsync(1, request);

        result.WorkerProfileId.Should().Be(42);
        result.WorkerServiceId.Should().BeNull();

        var inDb = await _context.Favorites.FirstAsync(f => f.CustomerId == 1);
        inDb.WorkerProfileId.Should().Be(42);
    }

    [Fact]
    public async Task AddAsync_Should_create_favorite_with_service()
    {
        var workerService = new Domain.Entities.WorkerService
        {
            WorkerProfileId = 1,
            Title = "Plumbing",
            PriceType = "Fixed",
            Price = 200m
        };
        _context.WorkerServices.Add(workerService);
        await _context.SaveChangesAsync();

        var request = new AddFavoriteRequest { WorkerServiceId = workerService.Id };

        var result = await _service.AddAsync(1, request);

        result.WorkerServiceId.Should().Be(workerService.Id);
        result.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task RemoveAsync_Should_return_false_when_not_found()
    {
        var result = await _service.RemoveAsync(1, 999);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveAsync_Should_return_false_for_wrong_customer()
    {
        var favorite = new Favorite { CustomerId = 1 };
        _context.Favorites.Add(favorite);
        await _context.SaveChangesAsync();

        var result = await _service.RemoveAsync(2, favorite.Id);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveAsync_Should_remove_and_return_true()
    {
        var favorite = new Favorite { CustomerId = 1 };
        _context.Favorites.Add(favorite);
        await _context.SaveChangesAsync();

        var result = await _service.RemoveAsync(1, favorite.Id);
        result.Should().BeTrue();

        var inDb = await _context.Favorites.FindAsync(favorite.Id);
        inDb.Should().BeNull();
    }

    [Fact]
    public async Task IsFavoritedAsync_Should_return_true_when_exists()
    {
        _context.Favorites.Add(new Favorite { CustomerId = 1, WorkerProfileId = 10, WorkerServiceId = 20 });
        await _context.SaveChangesAsync();

        var result = await _service.IsFavoritedAsync(1, 10, 20);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsFavoritedAsync_Should_return_false_when_not_exists()
    {
        var result = await _service.IsFavoritedAsync(1, 99, 99);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_Should_throw_on_duplicate_with_both_ids()
    {
        var request = new AddFavoriteRequest { WorkerProfileId = 1, WorkerServiceId = 2 };
        _context.Favorites.Add(new Favorite { CustomerId = 1, WorkerProfileId = 1, WorkerServiceId = 2 });
        await _context.SaveChangesAsync();

        var act = () => _service.AddAsync(1, request);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Already in favorites");
    }

    [Fact]
    public async Task IsFavoritedAsync_Should_match_null_ids_correctly()
    {
        _context.Favorites.Add(new Favorite { CustomerId = 1, WorkerProfileId = null, WorkerServiceId = null });
        await _context.SaveChangesAsync();

        var result = await _service.IsFavoritedAsync(1, null, null);
        result.Should().BeTrue();
    }
}
