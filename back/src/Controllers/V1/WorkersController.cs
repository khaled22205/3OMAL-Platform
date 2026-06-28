using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using src.DTOs.Workers;
using src.Services.Interfaces;

namespace src.Controllers.V1;

public class WorkersController : BaseApiController
{
    private readonly IWorkerService _workerService;

    public WorkersController(IWorkerService workerService)
    {
        _workerService = workerService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] WorkerSearchRequest request)
    {
        var results = await _workerService.SearchAsync(request);
        return OkResult(results);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var profile = await _workerService.GetProfileByIdAsync(id);
        if (profile == null) return NotFoundResult("Worker not found");
        return OkResult(profile);
    }

    [HttpGet("profile")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetUserId();
        var profile = await _workerService.GetProfileAsync(userId);
        if (profile == null) return NotFoundResult("Profile not found. Create one first.");
        return OkResult(profile);
    }

    [HttpPut("profile")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> UpdateProfile([FromBody] WorkerProfileRequest request)
    {
        var userId = GetUserId();
        var profile = await _workerService.CreateOrUpdateProfileAsync(userId, request);
        return OkResult(profile);
    }

    [HttpPatch("availability")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> UpdateAvailability([FromBody] WorkerStatusRequest request)
    {
        var userId = GetUserId();
        var result = await _workerService.UpdateAvailabilityStatusAsync(userId, request.IsAvailable);
        if (!result) return BadRequestResult("Failed to update availability");
        return OkResult(new { message = "Availability updated" });
    }

    [HttpPost("availability/slots")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> AddAvailabilitySlot([FromBody] WorkerAvailabilityRequest request)
    {
        var userId = GetUserId();
        var result = await _workerService.AddAvailabilityAsync(userId, request);
        return CreatedResult(result);
    }

    [HttpDelete("availability/slots/{availabilityId}")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> RemoveAvailabilitySlot(int availabilityId)
    {
        var userId = GetUserId();
        var result = await _workerService.RemoveAvailabilityAsync(userId, availabilityId);
        if (!result) return NotFoundResult("Availability slot not found");
        return OkResult(new { message = "Availability slot removed" });
    }

    [HttpPost("portfolio")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> AddPortfolioItem([FromBody] WorkerPortfolioRequest request)
    {
        var userId = GetUserId();
        var result = await _workerService.AddPortfolioItemAsync(userId, request);
        return CreatedResult(result);
    }

    [HttpDelete("portfolio/{portfolioItemId}")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> RemovePortfolioItem(int portfolioItemId)
    {
        var userId = GetUserId();
        var result = await _workerService.RemovePortfolioItemAsync(userId, portfolioItemId);
        if (!result) return NotFoundResult("Portfolio item not found");
        return OkResult(new { message = "Portfolio item removed" });
    }
}