using Application.Common.Models;

namespace Application.Features.Workers;

public interface IWorkerService
{
    Task<WorkerProfileResponse?> GetProfileAsync(int userId);
    Task<WorkerProfileResponse?> GetProfileByIdAsync(int profileId);
    Task<WorkerProfileResponse> CreateOrUpdateProfileAsync(int userId, WorkerProfileRequest request);
    Task<bool> UpdateAvailabilityStatusAsync(int userId, bool isAvailable);
    Task<PagedResult<WorkerSummaryResponse>> SearchAsync(WorkerSearchRequest request);
    Task<WorkerAvailabilityResponse> AddAvailabilityAsync(int userId, WorkerAvailabilityRequest request);
    Task<bool> RemoveAvailabilityAsync(int userId, int availabilityId);
    Task<WorkerPortfolioResponse> AddPortfolioItemAsync(int userId, WorkerPortfolioRequest request);
    Task<bool> RemovePortfolioItemAsync(int userId, int portfolioItemId);
}
