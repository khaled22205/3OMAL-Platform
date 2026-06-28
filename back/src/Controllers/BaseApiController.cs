using Microsoft.AspNetCore.Mvc;
using src.DTOs.Common;
using src.Services.Interfaces;

namespace src.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult OkResult<T>(T data, string? message = null) =>
        Ok(ApiResponse<T>.Ok(data, message));

    protected IActionResult CreatedResult<T>(T data, string? message = null) =>
        Created(string.Empty, ApiResponse<T>.Ok(data, message));

    protected IActionResult BadRequestResult(string message, List<string>? errors = null) =>
        BadRequest(ApiResponse<object>.Fail(message, errors));

    protected IActionResult NotFoundResult(string message = "Resource not found") =>
        NotFound(ApiResponse<object>.Fail(message));

    protected int GetUserId()
    {
        var service = HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
        return service.GetUserId()
            ?? throw new UnauthorizedAccessException("User not authenticated");
    }
}