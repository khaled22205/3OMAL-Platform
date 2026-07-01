using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Services;

namespace API.Controllers.V1;

public class ServicesController : BaseApiController
{
    private readonly IWorkerServiceService _serviceService;

    public ServicesController(IWorkerServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? searchTerm, [FromQuery] int? categoryId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var results = await _serviceService.SearchAsync(searchTerm, categoryId, page, pageSize);
        return OkResult(results);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var service = await _serviceService.GetByIdAsync(id);
        if (service == null) return NotFoundResult("Service not found");
        return OkResult(service);
    }

    [HttpGet("worker/{workerProfileId}")]
    public async Task<IActionResult> GetByWorker(int workerProfileId)
    {
        var services = await _serviceService.GetByWorkerAsync(workerProfileId);
        return OkResult(services);
    }

    [HttpPost]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> Create([FromBody] ServiceRequest request)
    {
        var userId = GetUserId();
        var service = await _serviceService.CreateAsync(userId, request);
        return CreatedResult(service);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> Update(int id, [FromBody] ServiceRequest request)
    {
        var userId = GetUserId();
        var service = await _serviceService.UpdateAsync(userId, id, request);
        return OkResult(service);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var result = await _serviceService.DeleteAsync(userId, id);
        if (!result) return NotFoundResult("Service not found");
        return OkResult(new { message = "Service deleted" });
    }

    [HttpPatch("{id}/toggle-active")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var userId = GetUserId();
        var result = await _serviceService.ToggleActiveAsync(userId, id);
        if (!result) return NotFoundResult("Service not found");
        return OkResult(new { message = "Service status toggled" });
    }
}
