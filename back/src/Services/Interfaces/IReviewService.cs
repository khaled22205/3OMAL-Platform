using src.DTOs.Common;
using src.DTOs.Reviews;

namespace src.Services.Interfaces;

public interface IReviewService
{
    Task<ReviewResponse?> GetByIdAsync(int id);
    Task<PagedResponse<ReviewResponse>> GetWorkerReviewsAsync(int workerProfileId, int page, int pageSize);
    Task<ReviewResponse> CreateAsync(int customerId, CreateReviewRequest request);
    Task<ReviewResponse> UpdateAsync(int customerId, int reviewId, UpdateReviewRequest request);
    Task<bool> ReplyAsync(int workerUserId, int reviewId, string reply);
    Task<bool> DeleteAsync(int customerId, int reviewId);
}