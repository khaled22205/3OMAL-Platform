using src.DTOs.Admin;
using src.DTOs.Common;

namespace src.Services.Interfaces;

public interface IAdminService
{
    Task<DashboardStatsResponse> GetDashboardStatsAsync();
    Task<PagedResponse<UserManagementResponse>> GetUsersAsync(int page, int pageSize, string? role = null, string? search = null);
    Task<UserManagementResponse?> GetUserByIdAsync(int id);
    Task<bool> UpdateUserAsync(int id, AdminUserUpdateRequest request);
    Task<bool> LockoutUserAsync(int id, DateTime? lockoutEnd);
    Task<bool> DeleteUserAsync(int id);
    Task<byte[]> ExportBookingsAsync(DateTime? from, DateTime? to);
    Task<byte[]> ExportUsersAsync(string? role = null);
}