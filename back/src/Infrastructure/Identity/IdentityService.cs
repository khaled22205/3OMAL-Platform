using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces;
using Application.Features.Admin;

namespace Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<IdentityUser<int>> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;

    public IdentityService(
        UserManager<IdentityUser<int>> userManager,
        RoleManager<IdentityRole<int>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email) != null;
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> CreateUserAsync(string email, string password, string phoneNumber)
    {
        var user = new IdentityUser<int>
        {
            UserName = email,
            Email = email,
            PhoneNumber = phoneNumber,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, password);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }

    public async Task<bool> CheckPasswordAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return false;
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<bool> IsLockedOutAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return false;
        return await _userManager.IsLockedOutAsync(user);
    }

    public async Task<IList<string>> GetUserRolesAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return [];
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<IList<string>> GetUserRolesByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return [];
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<bool> AddToRoleAsync(int userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;
        var result = await _userManager.AddToRoleAsync(user, role);
        return result.Succeeded;
    }

    public async Task<bool> AddToRoleByEmailAsync(string email, string role)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return false;
        var result = await _userManager.AddToRoleAsync(user, role);
        return result.Succeeded;
    }

    public async Task<bool> RoleExistsAsync(string role)
    {
        return await _roleManager.RoleExistsAsync(role);
    }

    public async Task<bool> CreateRoleAsync(string role)
    {
        var result = await _roleManager.CreateAsync(new IdentityRole<int>(role));
        return result.Succeeded;
    }

    public async Task<int?> GetUserIdByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user?.Id;
    }

    public async Task<string?> GetUserNameAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.UserName;
    }

    public async Task<string?> GetUserEmailAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.Email;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded;
    }

    public async Task<bool> VerifyEmailAsync(int userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;
        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public async Task<bool> UpdateUserAsync(int userId, string? email, string? phoneNumber)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        if (!string.IsNullOrWhiteSpace(email))
        {
            user.Email = email;
            user.UserName = email;
        }
        if (!string.IsNullOrWhiteSpace(phoneNumber))
            user.PhoneNumber = phoneNumber;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> UpdateUserRoleAsync(int userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        if (await _roleManager.RoleExistsAsync(role))
            await _userManager.AddToRoleAsync(user, role);

        return true;
    }

    public async Task<bool> SetLockoutEndDateAsync(int userId, DateTime? lockoutEnd)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        await _userManager.SetLockoutEndDateAsync(user,
            lockoutEnd.HasValue ? DateTimeOffset.Parse(lockoutEnd.Value.ToString("O")) : null);
        return true;
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public async Task<UserManagementResponse?> GetUserByIdManagedAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return null;

        var roles = (await GetUserRolesAsync(userId)).ToList();
        return new UserManagementResponse
        {
            Id = user.Id,
            Email = user.Email ?? "",
            UserName = user.UserName ?? "",
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            IsLockedOut = user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
            LockoutEnd = user.LockoutEnd?.UtcDateTime,
            CreatedAt = DateTime.UtcNow,
            Roles = roles
        };
    }

    public async Task<int> GetUserCountByRoleAsync(string role)
    {
        var users = await _userManager.GetUsersInRoleAsync(role);
        return users.Count;
    }

    public async Task<List<UserManagementResponse>> GetPagedUsersAsync(int page, int pageSize, string? role = null, string? search = null)
    {
        IList<IdentityUser<int>> users;

        if (!string.IsNullOrWhiteSpace(role))
        {
            users = await _userManager.GetUsersInRoleAsync(role);
        }
        else
        {
            users = await _userManager.Users.ToListAsync();
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            users = users.Where(u =>
                u.Email?.Contains(search, StringComparison.OrdinalIgnoreCase) == true ||
                u.UserName?.Contains(search, StringComparison.OrdinalIgnoreCase) == true).ToList();
        }

        var result = users
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserManagementResponse
            {
                Id = u.Id,
                Email = u.Email ?? "",
                UserName = u.UserName ?? "",
                PhoneNumber = u.PhoneNumber,
                EmailConfirmed = u.EmailConfirmed,
                PhoneNumberConfirmed = u.PhoneNumberConfirmed,
                IsLockedOut = u.LockoutEnabled && u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow,
                LockoutEnd = u.LockoutEnd?.UtcDateTime,
                CreatedAt = DateTime.UtcNow,
                Roles = new List<string>()
            })
            .ToList();

        foreach (var item in result)
        {
            item.Roles = (await GetUserRolesAsync(item.Id)).ToList();
        }

        return result;
    }
}
