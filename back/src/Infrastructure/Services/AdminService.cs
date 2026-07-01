using System.Data;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Admin;
using Infrastructure.Data;

namespace Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;
    private readonly IIdentityService _identity;
    private readonly UserManager<IdentityUser<int>> _userManager;

    public AdminService(AppDbContext context, IIdentityService identity, UserManager<IdentityUser<int>> userManager)
    {
        _context = context;
        _identity = identity;
        _userManager = userManager;
    }

    public async Task<DashboardStatsResponse> GetDashboardStatsAsync()
    {
        var totalUsers = await _identity.GetUserCountByRoleAsync("Customer") + await _identity.GetUserCountByRoleAsync("Worker");
        var workerIdsCount = await _identity.GetUserCountByRoleAsync("Worker");
        var customerIdsCount = await _identity.GetUserCountByRoleAsync("Customer");
        var totalBookings = await _context.Bookings.CountAsync();
        var activeBookings = await _context.Bookings.CountAsync(b =>
            b.Status == Domain.Enums.BookingStatus.Accepted ||
            b.Status == Domain.Enums.BookingStatus.Scheduled ||
            b.Status == Domain.Enums.BookingStatus.OnTheWay ||
            b.Status == Domain.Enums.BookingStatus.Started ||
            b.Status == Domain.Enums.BookingStatus.Paused);
        var completedBookings = await _context.Bookings.CountAsync(b => b.Status == Domain.Enums.BookingStatus.Completed);
        var totalRevenue = await _context.Payments
            .Where(p => p.Status == "Completed")
            .SumAsync(p => p.Amount);
        var totalCommission = await _context.Payments
            .Where(p => p.Status == "Completed")
            .SumAsync(p => p.CommissionAmount);
        var totalCategories = await _context.Categories.CountAsync();

        var monthlyBookings = await _context.Bookings
            .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
            .Select(g => new MonthlyStats { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
            .OrderByDescending(m => m.Year).ThenByDescending(m => m.Month)
            .Take(12)
            .ToListAsync();

        var topCategories = await _context.Bookings
            .Where(b => b.WorkerService != null)
            .Join(_context.WorkerServices, b => b.WorkerServiceId, s => s.Id, (b, s) => s.CategoryId)
            .Join(_context.Categories, cId => cId, c => c.Id, (cId, c) => c.Name)
            .GroupBy(n => n)
            .Select(g => new TopCategoryStats { CategoryName = g.Key, BookingCount = g.Count() })
            .OrderByDescending(t => t.BookingCount)
            .Take(5)
            .ToListAsync();

        return new DashboardStatsResponse
        {
            TotalUsers = totalUsers,
            TotalWorkers = workerIdsCount,
            TotalCustomers = customerIdsCount,
            TotalBookings = totalBookings,
            ActiveBookings = activeBookings,
            CompletedBookings = completedBookings,
            TotalRevenue = totalRevenue,
            TotalCommission = totalCommission,
            TotalCategories = totalCategories,
            MonthlyBookings = monthlyBookings,
            TopCategories = topCategories
        };
    }

    public async Task<PagedResult<UserManagementResponse>> GetUsersAsync(int page, int pageSize, string? role = null, string? search = null)
    {
        var users = await _identity.GetPagedUsersAsync(page, pageSize, role, search);
        var totalCount = role != null
            ? await _identity.GetUserCountByRoleAsync(role)
            : await _userManager.Users.CountAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            totalCount = users.Count;
        }

        return new PagedResult<UserManagementResponse>
        {
            Items = users,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<UserManagementResponse?> GetUserByIdAsync(int id)
    {
        var email = await _identity.GetUserEmailAsync(id);
        if (email == null) return null;

        var roles = (await _identity.GetUserRolesAsync(id)).ToList();
        return new UserManagementResponse
        {
            Id = id,
            Email = email,
            Roles = roles
        };
    }

    public async Task<bool> UpdateUserAsync(int id, AdminUserUpdateRequest request)
    {
        return await _identity.UpdateUserAsync(id, request.Email, request.PhoneNumber);
    }

    public async Task<bool> LockoutUserAsync(int id, DateTime? lockoutEnd)
    {
        return await _identity.SetLockoutEndDateAsync(id, lockoutEnd);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        return await _identity.DeleteUserAsync(id);
    }

    public async Task<byte[]> ExportBookingsAsync(DateTime? from, DateTime? to)
    {
        var query = _context.Bookings
            .Include(b => b.WorkerProfile)
            .Include(b => b.WorkerService)
            .AsQueryable();

        if (from.HasValue) query = query.Where(b => b.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(b => b.CreatedAt <= to.Value);

        var bookings = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Bookings");
        ws.Cell(1, 1).Value = "ID";
        ws.Cell(1, 2).Value = "Customer ID";
        ws.Cell(1, 3).Value = "Worker ID";
        ws.Cell(1, 4).Value = "Status";
        ws.Cell(1, 5).Value = "Total Price";
        ws.Cell(1, 6).Value = "Commission";
        ws.Cell(1, 7).Value = "Scheduled At";
        ws.Cell(1, 8).Value = "Created At";

        for (int i = 0; i < bookings.Count; i++)
        {
            var b = bookings[i];
            ws.Cell(i + 2, 1).Value = b.Id;
            ws.Cell(i + 2, 2).Value = b.CustomerId;
            ws.Cell(i + 2, 3).Value = b.WorkerProfileId;
            ws.Cell(i + 2, 4).Value = b.Status.ToString();
            ws.Cell(i + 2, 5).Value = (double)b.TotalPrice;
            ws.Cell(i + 2, 6).Value = (double)b.CommissionAmount;
            ws.Cell(i + 2, 7).Value = b.ScheduledAt.ToString("yyyy-MM-dd HH:mm");
            ws.Cell(i + 2, 8).Value = b.CreatedAt.ToString("yyyy-MM-dd HH:mm");
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportUsersAsync(string? role = null)
    {
        IList<IdentityUser<int>> users;
        if (!string.IsNullOrWhiteSpace(role))
            users = await _userManager.GetUsersInRoleAsync(role);
        else
            users = await _userManager.Users.ToListAsync();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Users");
        ws.Cell(1, 1).Value = "ID";
        ws.Cell(1, 2).Value = "Email";
        ws.Cell(1, 3).Value = "Phone";
        ws.Cell(1, 4).Value = "Email Confirmed";
        ws.Cell(1, 5).Value = "Locked Out";

        for (int i = 0; i < users.Count; i++)
        {
            var u = users[i];
            ws.Cell(i + 2, 1).Value = u.Id;
            ws.Cell(i + 2, 2).Value = u.Email;
            ws.Cell(i + 2, 3).Value = u.PhoneNumber;
            ws.Cell(i + 2, 4).Value = u.EmailConfirmed;
            ws.Cell(i + 2, 5).Value = u.LockoutEnabled && u.LockoutEnd > DateTimeOffset.UtcNow;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
