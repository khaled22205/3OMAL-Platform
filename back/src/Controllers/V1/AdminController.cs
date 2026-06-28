using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using src.DTOs.Admin;
using src.Services.Interfaces;

namespace src.Controllers.V1;

[Authorize(Roles = "Admin")]
public class AdminController : BaseApiController
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var stats = await _adminService.GetDashboardStatsAsync();
        return OkResult(stats);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? role = null, [FromQuery] string? search = null)
    {
        var users = await _adminService.GetUsersAsync(page, pageSize, role, search);
        return OkResult(users);
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _adminService.GetUserByIdAsync(id);
        if (user == null) return NotFoundResult("User not found");
        return OkResult(user);
    }

    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] AdminUserUpdateRequest request)
    {
        var result = await _adminService.UpdateUserAsync(id, request);
        if (!result) return NotFoundResult("User not found");
        return OkResult(new { message = "User updated" });
    }

    [HttpPost("users/{id}/lock")]
    public async Task<IActionResult> LockUser(int id, [FromQuery] DateTime? lockoutEnd)
    {
        var result = await _adminService.LockoutUserAsync(id, lockoutEnd);
        if (!result) return NotFoundResult("User not found");
        return OkResult(new { message = lockoutEnd.HasValue ? "User locked" : "User unlocked" });
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _adminService.DeleteUserAsync(id);
        if (!result) return NotFoundResult("User not found");
        return OkResult(new { message = "User deleted" });
    }

    [HttpGet("export/bookings")]
    public async Task<IActionResult> ExportBookings([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var data = await _adminService.ExportBookingsAsync(from, to);
        return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "bookings.xlsx");
    }

    [HttpGet("export/users")]
    public async Task<IActionResult> ExportUsers([FromQuery] string? role = null)
    {
        var data = await _adminService.ExportUsersAsync(role);
        return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "users.xlsx");
    }
}