using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using src.Data;
using src.DTOs.Common;
using src.DTOs.Services;
using src.Helpers;
using src.Services.Interfaces;
using WorkerServiceModel = src.Models.WorkerService;
using ServiceImageModel = src.Models.ServiceImage;

namespace src.Services.Implementations;

public class WorkerServiceService : IWorkerServiceService
{
    private readonly AppDbContext _context;
    private readonly UserManager<IdentityUser<int>> _userManager;

    public WorkerServiceService(AppDbContext context, UserManager<IdentityUser<int>> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<ServiceResponse?> GetByIdAsync(int id)
    {
        var service = await _context.WorkerServices
            .Include(s => s.Category)
            .Include(s => s.Images)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (service == null) return null;

        var worker = await _context.WorkerProfiles.FindAsync(service.WorkerProfileId);
        var user = worker != null ? await _userManager.FindByIdAsync(worker.UserId.ToString()) : null;

        return service.ToResponse(
            workerName: user?.UserName ?? "",
            categoryName: service.Category?.Name ?? "",
            images: service.Images.Select(i => i.ImageUrl).ToList()
        );
    }

    public async Task<List<ServiceResponse>> GetByWorkerAsync(int workerProfileId)
    {
        var services = await _context.WorkerServices
            .Include(s => s.Category)
            .Include(s => s.Images)
            .Where(s => s.WorkerProfileId == workerProfileId)
            .ToListAsync();

        var worker = await _context.WorkerProfiles.FindAsync(workerProfileId);
        var user = worker != null ? await _userManager.FindByIdAsync(worker.UserId.ToString()) : null;

        return services.Select(s => s.ToResponse(
            workerName: user?.UserName ?? "",
            categoryName: s.Category?.Name ?? "",
            images: s.Images.Select(i => i.ImageUrl).ToList()
        )).ToList();
    }

    public async Task<PagedResponse<ServiceResponse>> SearchAsync(string? searchTerm, int? categoryId, int page, int pageSize)
    {
        var query = _context.WorkerServices
            .Include(s => s.Category)
            .Include(s => s.Images)
            .Where(s => s.IsActive);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(s => s.Title.ToLower().Contains(term) ||
                                     (s.Description != null && s.Description.ToLower().Contains(term)) ||
                                     (s.Tags != null && s.Tags.ToLower().Contains(term)));
        }

        if (categoryId.HasValue)
            query = query.Where(s => s.CategoryId == categoryId.Value);

        var totalCount = await query.CountAsync();
        var services = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = new List<ServiceResponse>();
        foreach (var service in services)
        {
            var worker = await _context.WorkerProfiles.FindAsync(service.WorkerProfileId);
            var user = worker != null ? await _userManager.FindByIdAsync(worker.UserId.ToString()) : null;

            items.Add(service.ToResponse(
                workerName: user?.UserName ?? "",
                categoryName: service.Category?.Name ?? "",
                images: service.Images.Select(i => i.ImageUrl).ToList()
            ));
        }

        return new PagedResponse<ServiceResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ServiceResponse> CreateAsync(int userId, ServiceRequest request)
    {
        var profile = await _context.WorkerProfiles
            .FirstOrDefaultAsync(w => w.UserId == userId)
            ?? throw new InvalidOperationException("Worker profile not found");

        if (!await _context.Categories.AnyAsync(c => c.Id == request.CategoryId))
            throw new ArgumentException("Invalid category");

        var service = new WorkerServiceModel
        {
            WorkerProfileId = profile.Id,
            CategoryId = request.CategoryId,
            Title = request.Title,
            Description = request.Description,
            PriceType = request.PriceType,
            Price = request.Price,
            EstimatedDurationMinutes = request.EstimatedDurationMinutes,
            MaterialsIncluded = request.MaterialsIncluded,
            AvailableCities = request.AvailableCities,
            Tags = request.Tags,
            IsActive = true
        };

        _context.WorkerServices.Add(service);
        await _context.SaveChangesAsync();

        if (request.Images.Count != 0)
        {
            foreach (var img in request.Images)
            {
                _context.ServiceImages.Add(new ServiceImageModel
                {
                    WorkerServiceId = service.Id,
                    ImageUrl = img
                });
            }
            await _context.SaveChangesAsync();
        }

        var category = await _context.Categories.FindAsync(request.CategoryId);
        return service.ToResponse(categoryName: category?.Name ?? "",
            images: request.Images.ToList());
    }

    public async Task<ServiceResponse> UpdateAsync(int userId, int serviceId, ServiceRequest request)
    {
        var profile = await _context.WorkerProfiles
            .FirstOrDefaultAsync(w => w.UserId == userId)
            ?? throw new InvalidOperationException("Worker profile not found");

        var service = await _context.WorkerServices
            .Include(s => s.Images)
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.WorkerProfileId == profile.Id)
            ?? throw new KeyNotFoundException("Service not found");

        service.Title = request.Title;
        service.Description = request.Description;
        service.PriceType = request.PriceType;
        service.Price = request.Price;
        service.EstimatedDurationMinutes = request.EstimatedDurationMinutes;
        service.MaterialsIncluded = request.MaterialsIncluded;
        service.AvailableCities = request.AvailableCities;
        service.Tags = request.Tags;
        service.CategoryId = request.CategoryId;
        service.UpdatedAt = DateTime.UtcNow;

        _context.ServiceImages.RemoveRange(service.Images);
        foreach (var img in request.Images)
        {
            _context.ServiceImages.Add(new ServiceImageModel
            {
                WorkerServiceId = service.Id,
                ImageUrl = img
            });
        }

        await _context.SaveChangesAsync();

        var category = await _context.Categories.FindAsync(request.CategoryId);
        return service.ToResponse(categoryName: category?.Name ?? "",
            images: request.Images.ToList());
    }

    public async Task<bool> DeleteAsync(int userId, int serviceId)
    {
        var profile = await _context.WorkerProfiles
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (profile == null) return false;

        var service = await _context.WorkerServices
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.WorkerProfileId == profile.Id);

        if (service == null) return false;

        service.IsDeleted = true;
        service.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleActiveAsync(int userId, int serviceId)
    {
        var profile = await _context.WorkerProfiles
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (profile == null) return false;

        var service = await _context.WorkerServices
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.WorkerProfileId == profile.Id);

        if (service == null) return false;

        service.IsActive = !service.IsActive;
        service.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}