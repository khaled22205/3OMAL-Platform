using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Reviews;
using Application.Common.Mappings;
using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Services;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;
    private readonly IIdentityService _identity;

    public ReviewService(AppDbContext context, IIdentityService identity)
    {
        _context = context;
        _identity = identity;
    }

    public async Task<ReviewResponse?> GetByIdAsync(int id)
    {
        var review = await _context.Reviews
            .Include(r => r.Booking)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review == null) return null;

        return await BuildResponseAsync(review);
    }

    public async Task<PagedResult<ReviewResponse>> GetWorkerReviewsAsync(int workerProfileId, int page, int pageSize)
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

        return new PagedResult<ReviewResponse>
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

        if (booking.Status != Domain.Enums.BookingStatus.Completed)
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

        review.UpdateRating(request.Rating, request.Comment);

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

        review.Reply(reply);
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
        var customerName = await _identity.GetUserNameAsync(review.CustomerId) ?? "";
        var worker = await _context.WorkerProfiles.FindAsync(review.WorkerProfileId);
        var workerName = worker != null ? await _identity.GetUserNameAsync(worker.UserId) ?? "" : "";

        return review.ToResponse(customerName, workerName: workerName);
    }

    private async Task UpdateWorkerRating(int workerProfileId)
    {
        var avg = await _context.Reviews
            .Where(r => r.WorkerProfileId == workerProfileId)
            .AverageAsync(r => (double)r.Rating);

        var worker = await _context.WorkerProfiles.FindAsync(workerProfileId);
        if (worker != null)
        {
            worker.UpdateRating(avg);
            await _context.SaveChangesAsync();
        }
    }
}
