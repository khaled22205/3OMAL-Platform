using Microsoft.AspNetCore.Mvc;
using Application.Common.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult OkResult<T>(T data, string? message = null) =>
        Ok(new { success = true, data, message });

    protected IActionResult CreatedResult<T>(T data, string? message = null) =>
        Created(string.Empty, new { success = true, data, message });

    protected IActionResult BadRequestResult(string message, List<string>? errors = null) =>
        BadRequest(new { success = false, message, errors });

    protected IActionResult NotFoundResult(string message = "Resource not found") =>
        NotFound(new { success = false, message });

    protected int GetUserId()
    {
        var service = HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
        return service.GetUserId()
            ?? throw new UnauthorizedAccessException("User not authenticated");
    }
}
