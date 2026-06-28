using Microsoft.EntityFrameworkCore;
using src.Data;
using src.DTOs.Common;
using src.DTOs.Favorites;
using src.Helpers;
using src.Models;
using src.Services.Interfaces;

namespace src.Services.Implementations;

public class FavoriteService : IFavoriteService
{
    private readonly AppDbContext _context;

    public FavoriteService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<FavoriteResponse>> GetUserFavoritesAsync(int customerId, int page, int pageSize)
    {
        var query = _context.Favorites
            .Include(f => f.WorkerProfile)
            .Include(f => f.WorkerService)
            .Where(f => f.CustomerId == customerId)
            .OrderByDescending(f => f.CreatedAt);

        var totalCount = await query.CountAsync();
        var favorites = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = favorites.Select(f => new FavoriteResponse
        {
            Id = f.Id,
            WorkerProfileId = f.WorkerProfileId,
            WorkerName = f.WorkerProfileId.HasValue ? $"Worker #{f.WorkerProfileId}" : null,
            WorkerServiceId = f.WorkerServiceId,
            ServiceName = f.WorkerService?.Title,
            ServicePrice = f.WorkerService?.Price,
            CreatedAt = f.CreatedAt
        }).ToList();

        return new PagedResponse<FavoriteResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<FavoriteResponse> AddAsync(int customerId, AddFavoriteRequest request)
    {
        if (!request.WorkerProfileId.HasValue && !request.WorkerServiceId.HasValue)
            throw new ArgumentException("Specify a worker or service to favorite");

        var exists = await _context.Favorites.AnyAsync(f =>
            f.CustomerId == customerId &&
            f.WorkerProfileId == request.WorkerProfileId &&
            f.WorkerServiceId == request.WorkerServiceId);

        if (exists)
            throw new InvalidOperationException("Already in favorites");

        var favorite = new Favorite
        {
            CustomerId = customerId,
            WorkerProfileId = request.WorkerProfileId,
            WorkerServiceId = request.WorkerServiceId
        };

        _context.Favorites.Add(favorite);
        await _context.SaveChangesAsync();

        return new FavoriteResponse
        {
            Id = favorite.Id,
            WorkerProfileId = favorite.WorkerProfileId,
            WorkerServiceId = favorite.WorkerServiceId,
            CreatedAt = favorite.CreatedAt
        };
    }

    public async Task<bool> RemoveAsync(int customerId, int favoriteId)
    {
        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.Id == favoriteId && f.CustomerId == customerId);

        if (favorite == null) return false;

        _context.Favorites.Remove(favorite);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsFavoritedAsync(int customerId, int? workerProfileId, int? serviceId)
    {
        return await _context.Favorites.AnyAsync(f =>
            f.CustomerId == customerId &&
            f.WorkerProfileId == workerProfileId &&
            f.WorkerServiceId == serviceId);
    }
}