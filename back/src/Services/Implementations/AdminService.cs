using System.Data;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using src.Data;
using src.DTOs.Admin;
using src.DTOs.Common;
using src.Helpers;
using src.Services.Interfaces;

namespace src.Services.Implementations;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;
    private readonly UserManager<IdentityUser<int>> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;

    public AdminService(AppDbContext context, UserManager<IdentityUser<int>> userManager, RoleManager<IdentityRole<int>> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<DashboardStatsResponse> GetDashboardStatsAsync()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var users = await _userManager.Users.ToListAsync();

        var workerIds = new List<int>();
        var customerIds = new List<int>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Worker")) workerIds.Add(user.Id);
            if (roles.Contains("Customer")) customerIds.Add(user.Id);
        }

        var totalBookings = await _context.Bookings.CountAsync();
        var activeBookings = await _context.Bookings.CountAsync(b =>
            b.Status == "Accepted" || b.Status == "Scheduled" ||
            b.Status == "OnTheWay" || b.Status == "Started" || b.Status == "Paused");
        var completedBookings = await _context.Bookings.CountAsync(b => b.Status == "Completed");
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
            TotalWorkers = workerIds.Count,
            TotalCustomers = customerIds.Count,
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

    public async Task<PagedResponse<UserManagementResponse>> GetUsersAsync(int page, int pageSize, string? role = null, string? search = null)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(u => u.Email!.ToLower().Contains(term) ||
                                     u.UserName!.ToLower().Contains(term) ||
                                     (u.PhoneNumber != null && u.PhoneNumber.Contains(term)));
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = new List<UserManagementResponse>();
        foreach (var user in users)
        {
            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            if (!string.IsNullOrWhiteSpace(role) && !roles.Contains(role))
                continue;

            items.Add(user.ToManagementResponse(roles));
        }

        return new PagedResponse<UserManagementResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<UserManagementResponse?> GetUserByIdAsync(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return null;

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        return user.ToManagementResponse(roles);
    }

    public async Task<bool> UpdateUserAsync(int id, AdminUserUpdateRequest request)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return false;

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            user.Email = request.Email;
            user.UserName = request.Email;
        }
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            user.PhoneNumber = request.PhoneNumber;

        await _userManager.UpdateAsync(user);

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (await _roleManager.RoleExistsAsync(request.Role))
                await _userManager.AddToRoleAsync(user, request.Role);
        }

        if (request.LockoutEnabled.HasValue && request.LockoutEnabled.Value)
        {
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        }
        else if (request.LockoutEnabled.HasValue && !request.LockoutEnabled.Value)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
        }

        return true;
    }

    public async Task<bool> LockoutUserAsync(int id, DateTime? lockoutEnd)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return false;

        await _userManager.SetLockoutEndDateAsync(user, lockoutEnd.HasValue ? DateTimeOffset.Parse(lockoutEnd.Value.ToString("O")) : null);
        return true;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
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
            ws.Cell(i + 2, 4).Value = b.Status;
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
        var users = await _userManager.Users.ToListAsync();
        if (!string.IsNullOrWhiteSpace(role))
        {
            var filtered = new List<IdentityUser<int>>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains(role))
                    filtered.Add(user);
            }
            users = filtered;
        }

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
            ws.Cell(i + 2, 4).Value = u.EmailConfirmed ? "Yes" : "No";
            ws.Cell(i + 2, 5).Value = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow ? "Yes" : "No";
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}