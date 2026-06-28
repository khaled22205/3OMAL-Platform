using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using src.Data;
using src.DTOs.Common;
using src.DTOs.Workers;
using src.Helpers;
using src.Models;
using src.Services.Interfaces;

namespace src.Services.Implementations;

public class WorkerService : IWorkerService
{
    private readonly AppDbContext _context;
    private readonly UserManager<IdentityUser<int>> _userManager;

    public WorkerService(AppDbContext context, UserManager<IdentityUser<int>> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<WorkerProfileResponse?> GetProfileAsync(int userId)
    {
        var profile = await _context.WorkerProfiles
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (profile == null) return null;

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return null;

        var availabilities = await _context.WorkerAvailabilities
            .Where(a => a.WorkerProfileId == profile.Id)
            .ToListAsync();

        var portfolio = await _context.WorkerPortfolioItems
            .Where(p => p.WorkerProfileId == profile.Id)
            .ToListAsync();

        return profile.ToResponse(user, availabilities, portfolio);
    }

    public async Task<WorkerProfileResponse?> GetProfileByIdAsync(int profileId)
    {
        var profile = await _context.WorkerProfiles.FindAsync(profileId);
        if (profile == null) return null;

        return await GetProfileAsync(profile.UserId);
    }

    public async Task<WorkerProfileResponse> CreateOrUpdateProfileAsync(int userId, WorkerProfileRequest request)
    {
        var profile = await _context.WorkerProfiles
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (profile == null)
        {
            profile = new WorkerProfile { UserId = userId };
            _context.WorkerProfiles.Add(profile);
        }

        profile.Photo = request.Photo;
        profile.CoverPhoto = request.CoverPhoto;
        profile.Biography = request.Biography;
        profile.YearsOfExperience = request.YearsOfExperience;
        profile.Skills = request.Skills;
        profile.ServiceAreas = request.ServiceAreas;
        profile.HourlyRate = request.HourlyRate;
        profile.StartingPrice = request.StartingPrice;
        profile.MinimumJobValue = request.MinimumJobValue;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var user = await _userManager.FindByIdAsync(userId.ToString())!;
        return profile.ToResponse(user!);
    }

    public async Task<bool> UpdateAvailabilityStatusAsync(int userId, bool isAvailable)
    {
        var profile = await _context.WorkerProfiles
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (profile == null) return false;

        profile.IsAvailable = isAvailable;
        profile.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResponse<WorkerSummaryResponse>> SearchAsync(WorkerSearchRequest request)
    {
        var workersQuery = _context.WorkerProfiles
            .Where(w => w.IsAvailable);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            workersQuery = workersQuery.Where(w =>
                w.Biography!.ToLower().Contains(term) ||
                w.Skills!.ToLower().Contains(term) ||
                w.ServiceAreas!.ToLower().Contains(term));
        }

        if (request.MinRating.HasValue)
            workersQuery = workersQuery.Where(w => w.AverageRating >= request.MinRating.Value);

        if (request.MaxPrice.HasValue)
            workersQuery = workersQuery.Where(w => w.StartingPrice <= request.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(request.City))
            workersQuery = workersQuery.Where(w => w.ServiceAreas!.ToLower().Contains(request.City.ToLower()));

        if (request.MinExperience.HasValue)
            workersQuery = workersQuery.Where(w => w.YearsOfExperience >= request.MinExperience.Value);

        request.SortBy = request.SortBy?.ToLower();
        workersQuery = request.SortBy switch
        {
            "cheapest" => workersQuery.OrderBy(w => w.StartingPrice),
            "highestrated" => workersQuery.OrderByDescending(w => w.AverageRating),
            "mostexperienced" => workersQuery.OrderByDescending(w => w.YearsOfExperience),
            "mostjobs" => workersQuery.OrderByDescending(w => w.CompletedJobs),
            "newest" => workersQuery.OrderByDescending(w => w.CreatedAt),
            _ => workersQuery.OrderByDescending(w => w.AverageRating)
        };

        var totalCount = await workersQuery.CountAsync();
        var workers = await workersQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var items = new List<WorkerSummaryResponse>();
        foreach (var worker in workers)
        {
            var user = await _userManager.FindByIdAsync(worker.UserId.ToString());
            if (user == null) continue;

            var serviceCategories = await _context.WorkerServices
                .Where(s => s.WorkerProfileId == worker.Id)
                .Include(s => s.Category)
                .Select(s => s.Category.Name)
                .Distinct()
                .ToListAsync();

            items.Add(worker.ToSummary(user, serviceCategories));
        }

        return new PagedResponse<WorkerSummaryResponse>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<WorkerAvailabilityResponse> AddAvailabilityAsync(int userId, WorkerAvailabilityRequest request)
    {
        var profile = await _context.WorkerProfiles
            .FirstOrDefaultAsync(w => w.UserId == userId)
            ?? throw new InvalidOperationException("Worker profile not found");

        var availability = new WorkerAvailability
        {
            WorkerProfileId = profile.Id,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsAvailable = request.IsAvailable
        };

        _context.WorkerAvailabilities.Add(availability);
        await _context.SaveChangesAsync();

        return availability.ToResponse();
    }

    public async Task<bool> RemoveAvailabilityAsync(int userId, int availabilityId)
    {
        var profile = await _context.WorkerProfiles
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (profile == null) return false;

        var availability = await _context.WorkerAvailabilities
            .FirstOrDefaultAsync(a => a.Id == availabilityId && a.WorkerProfileId == profile.Id);

        if (availability == null) return false;

        _context.WorkerAvailabilities.Remove(availability);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<WorkerPortfolioResponse> AddPortfolioItemAsync(int userId, WorkerPortfolioRequest request)
    {
        var profile = await _context.WorkerProfiles
            .FirstOrDefaultAsync(w => w.UserId == userId)
            ?? throw new InvalidOperationException("Worker profile not found");

        var item = new WorkerPortfolioItem
        {
            WorkerProfileId = profile.Id,
            MediaType = request.MediaType,
            MediaUrl = request.MediaUrl,
            Title = request.Title
        };

        _context.WorkerPortfolioItems.Add(item);
        await _context.SaveChangesAsync();

        return item.ToResponse();
    }

    public async Task<bool> RemovePortfolioItemAsync(int userId, int portfolioItemId)
    {
        var profile = await _context.WorkerProfiles
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (profile == null) return false;

        var item = await _context.WorkerPortfolioItems
            .FirstOrDefaultAsync(p => p.Id == portfolioItemId && p.WorkerProfileId == profile.Id);

        if (item == null) return false;

        _context.WorkerPortfolioItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }
}