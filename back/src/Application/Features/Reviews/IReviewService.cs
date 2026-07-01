using Application.Common.Models;

namespace Application.Features.Reviews;

public interface IReviewService
{
    Task<ReviewResponse?> GetByIdAsync(int id);
    Task<PagedResult<ReviewResponse>> GetWorkerReviewsAsync(int workerProfileId, int page, int pageSize);
    Task<ReviewResponse> CreateAsync(int customerId, CreateReviewRequest request);
    Task<ReviewResponse> UpdateAsync(int customerId, int reviewId, UpdateReviewRequest request);
    Task<bool> ReplyAsync(int workerUserId, int reviewId, string reply);
    Task<bool> DeleteAsync(int customerId, int reviewId);
}
