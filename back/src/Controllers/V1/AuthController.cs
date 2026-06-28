using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using src.DTOs.Auth;
using src.Services.Interfaces;

namespace src.Controllers.V1;

public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IAuthService authService, ICurrentUserService currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.Success) return BadRequestResult(result.Message ?? "Registration failed");
        return CreatedResult(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.Success) return BadRequestResult(result.Message ?? "Login failed");
        return OkResult(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        if (!result.Success) return BadRequestResult(result.Message ?? "Token refresh failed");
        return OkResult(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetUserId();
        var result = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
        if (!result) return BadRequestResult("Password change failed");
        return OkResult(new { message = "Password changed successfully" });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = GetUserId();
        await _authService.LogoutAsync(userId);
        return OkResult(new { message = "Logged out successfully" });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        return OkResult(new
        {
            userId = _currentUser.GetUserId(),
            email = _currentUser.GetUserEmail(),
            roles = _currentUser.GetUserRoles()
        });
    }
}