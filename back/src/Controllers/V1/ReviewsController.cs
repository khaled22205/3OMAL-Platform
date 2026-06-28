using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using src.DTOs.Reviews;
using src.Services.Interfaces;

namespace src.Controllers.V1;

public class ReviewsController : BaseApiController
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("worker/{workerProfileId}")]
    public async Task<IActionResult> GetWorkerReviews(int workerProfileId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var reviews = await _reviewService.GetWorkerReviewsAsync(workerProfileId, page, pageSize);
        return OkResult(reviews);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var review = await _reviewService.GetByIdAsync(id);
        if (review == null) return NotFoundResult("Review not found");
        return OkResult(review);
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Create([FromBody] CreateReviewRequest request)
    {
        var userId = GetUserId();
        var review = await _reviewService.CreateAsync(userId, request);
        return CreatedResult(review);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewRequest request)
    {
        var userId = GetUserId();
        var review = await _reviewService.UpdateAsync(userId, id, request);
        return OkResult(review);
    }

    [HttpPost("{id}/reply")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> Reply(int id, [FromBody] WorkerReplyRequest request)
    {
        var userId = GetUserId();
        var result = await _reviewService.ReplyAsync(userId, id, request.Reply);
        if (!result) return NotFoundResult("Review not found");
        return OkResult(new { message = "Reply added" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var result = await _reviewService.DeleteAsync(userId, id);
        if (!result) return NotFoundResult("Review not found");
        return OkResult(new { message = "Review deleted" });
    }
}