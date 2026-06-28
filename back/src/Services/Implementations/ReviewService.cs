using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using src.Data;
using src.DTOs.Common;
using src.DTOs.Reviews;
using src.Helpers;
using src.Models;
using src.Services.Interfaces;

namespace src.Services.Implementations;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;
    private readonly UserManager<IdentityUser<int>> _userManager;

    public ReviewService(AppDbContext context, UserManager<IdentityUser<int>> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<ReviewResponse?> GetByIdAsync(int id)
    {
        var review = await _context.Reviews
            .Include(r => r.Booking)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review == null) return null;

        return await BuildResponseAsync(review);
    }

    public async Task<PagedResponse<ReviewResponse>> GetWorkerReviewsAsync(int workerProfileId, int page, int pageSize)
    {
        var query = _context.Reviews
            .Include(r => r.Booking)
            .Where(r => r.WorkerProfileId == workerProfileId)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync();
        var reviews = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = new List<ReviewResponse>();
        foreach (var review in reviews)
            items.Add(await BuildResponseAsync(review));

        return new PagedResponse<ReviewResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ReviewResponse> CreateAsync(int customerId, CreateReviewRequest request)
    {
        var booking = await _context.Bookings.FindAsync(request.BookingId)
            ?? throw new ArgumentException("Booking not found");

        if (booking.CustomerId != customerId)
            throw new UnauthorizedAccessException("You can only review your own bookings");

        if (booking.Status != "Completed")
            throw new InvalidOperationException("Can only review completed bookings");

        var existingReview = await _context.Reviews
            .AnyAsync(r => r.BookingId == request.BookingId);
        if (existingReview)
            throw new InvalidOperationException("Booking already reviewed");

        if (request.Rating < 1 || request.Rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5");

        var review = new Review
        {
            BookingId = request.BookingId,
            CustomerId = customerId,
            WorkerProfileId = booking.WorkerProfileId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        await UpdateWorkerRating(booking.WorkerProfileId);

        return await BuildResponseAsync(review);
    }

    public async Task<ReviewResponse> UpdateAsync(int customerId, int reviewId, UpdateReviewRequest request)
    {
        var review = await _context.Reviews.FindAsync(reviewId)
            ?? throw new KeyNotFoundException("Review not found");

        if (review.CustomerId != customerId)
            throw new UnauthorizedAccessException("You can only edit your own reviews");

        if (request.Rating < 1 || request.Rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5");

        review.Rating = request.Rating;
        review.Comment = request.Comment;
        review.IsEdited = true;
        review.EditedAt = DateTime.UtcNow;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await UpdateWorkerRating(review.WorkerProfileId);

        return await BuildResponseAsync(review);
    }

    public async Task<bool> ReplyAsync(int workerUserId, int reviewId, string reply)
    {
        var profile = await _context.WorkerProfiles
            .FirstOrDefaultAsync(w => w.UserId == workerUserId)
            ?? throw new InvalidOperationException("Worker profile not found");

        var review = await _context.Reviews.FindAsync(reviewId)
            ?? throw new KeyNotFoundException("Review not found");

        if (review.WorkerProfileId != profile.Id)
            throw new UnauthorizedAccessException("Not your review to reply to");

        review.WorkerReply = reply;
        review.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int customerId, int reviewId)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review == null || review.CustomerId != customerId)
            return false;

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        await UpdateWorkerRating(review.WorkerProfileId);
        return true;
    }

    private async Task<ReviewResponse> BuildResponseAsync(Review review)
    {
        var customer = await _userManager.FindByIdAsync(review.CustomerId.ToString());
        var worker = await _context.WorkerProfiles.FindAsync(review.WorkerProfileId);
        var workerUser = worker != null ? await _userManager.FindByIdAsync(worker.UserId.ToString()) : null;

        return review.ToResponse(
            customerName: $"{customer?.UserName}",
            workerName: workerUser?.UserName ?? ""
        );
    }

    private async Task UpdateWorkerRating(int workerProfileId)
    {
        var avg = await _context.Reviews
            .Where(r => r.WorkerProfileId == workerProfileId)
            .AverageAsync(r => (double)r.Rating);

        var worker = await _context.WorkerProfiles.FindAsync(workerProfileId);
        if (worker != null)
        {
            worker.AverageRating = Math.Round(avg, 1);
            worker.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}