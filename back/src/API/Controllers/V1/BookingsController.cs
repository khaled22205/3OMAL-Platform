using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Bookings;

namespace API.Controllers.V1;

[Authorize]
public class BookingsController : BaseApiController
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var booking = await _bookingService.GetByIdAsync(id);
        if (booking == null) return NotFoundResult("Booking not found");
        return OkResult(booking);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyBookings([FromQuery] BookingFilterRequest filter)
    {
        var userId = GetUserId();
        var bookings = await _bookingService.GetCustomerBookingsAsync(userId, filter);
        return OkResult(bookings);
    }

    [HttpGet("worker/{workerProfileId}")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> GetWorkerBookings(int workerProfileId, [FromQuery] BookingFilterRequest filter)
    {
        var bookings = await _bookingService.GetWorkerBookingsAsync(workerProfileId, filter);
        return OkResult(bookings);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
    {
        var userId = GetUserId();
        var booking = await _bookingService.CreateAsync(userId, request);
        return CreatedResult(booking);
    }

    [HttpPatch("{id}/accept")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> Accept(int id)
    {
        var userId = GetUserId();
        var booking = await _bookingService.AcceptAsync(userId, id);
        return OkResult(booking);
    }

    [HttpPatch("{id}/reject")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> Reject(int id, [FromQuery] string? reason = null)
    {
        var userId = GetUserId();
        var booking = await _bookingService.RejectAsync(userId, id, reason);
        return OkResult(booking);
    }

    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromQuery] string? reason = null)
    {
        var userId = GetUserId();
        var booking = await _bookingService.CancelAsync(userId, id, reason);
        return OkResult(booking);
    }

    [HttpPatch("{id}/reschedule")]
    public async Task<IActionResult> Reschedule(int id, [FromQuery] DateTime newScheduledAt)
    {
        var userId = GetUserId();
        var booking = await _bookingService.RescheduleAsync(userId, id, newScheduledAt);
        return OkResult(booking);
    }

    [HttpPatch("{id}/on-the-way")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> MarkOnTheWay(int id)
    {
        var userId = GetUserId();
        var booking = await _bookingService.MarkOnTheWayAsync(userId, id);
        return OkResult(booking);
    }

    [HttpPatch("{id}/start")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> StartJob(int id)
    {
        var userId = GetUserId();
        var booking = await _bookingService.StartJobAsync(userId, id);
        return OkResult(booking);
    }

    [HttpPatch("{id}/pause")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> PauseJob(int id)
    {
        var userId = GetUserId();
        var booking = await _bookingService.PauseJobAsync(userId, id);
        return OkResult(booking);
    }

    [HttpPatch("{id}/complete")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> CompleteJob(int id)
    {
        var userId = GetUserId();
        var booking = await _bookingService.CompleteJobAsync(userId, id);
        return OkResult(booking);
    }
}
