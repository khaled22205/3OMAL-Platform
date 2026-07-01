namespace Application.Common.Interfaces;

public interface IIdentityService
{
    Task<bool> UserExistsAsync(string email);
    Task<(bool Success, IEnumerable<string> Errors)> CreateUserAsync(string email, string password, string phoneNumber);
    Task<bool> CheckPasswordAsync(string email, string password);
    Task<bool> IsLockedOutAsync(string email);
    Task<IList<string>> GetUserRolesAsync(int userId);
    Task<IList<string>> GetUserRolesByEmailAsync(string email);
    Task<bool> AddToRoleAsync(int userId, string role);
    Task<bool> AddToRoleByEmailAsync(string email, string role);
    Task<bool> RoleExistsAsync(string role);
    Task<bool> CreateRoleAsync(string role);
    Task<int?> GetUserIdByEmailAsync(string email);
    Task<string?> GetUserNameAsync(int userId);
    Task<string?> GetUserEmailAsync(int userId);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<bool> VerifyEmailAsync(int userId, string token);
    Task<bool> UpdateUserAsync(int userId, string? email, string? phoneNumber);
    Task<bool> UpdateUserRoleAsync(int userId, string role);
    Task<bool> SetLockoutEndDateAsync(int userId, DateTime? lockoutEnd);
    Task<bool> DeleteUserAsync(int userId);
    Task<Application.Features.Admin.UserManagementResponse?> GetUserByIdManagedAsync(int userId);
    Task<int> GetUserCountByRoleAsync(string role);
    Task<List<Application.Features.Admin.UserManagementResponse>> GetPagedUsersAsync(int page, int pageSize, string? role = null, string? search = null);
}
