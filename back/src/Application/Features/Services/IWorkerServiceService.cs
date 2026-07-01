using Application.Common.Models;

namespace Application.Features.Services;

public interface IWorkerServiceService
{
    Task<ServiceResponse?> GetByIdAsync(int id);
    Task<List<ServiceResponse>> GetByWorkerAsync(int workerProfileId);
    Task<PagedResult<ServiceResponse>> SearchAsync(string? searchTerm, int? categoryId, int page, int pageSize);
    Task<ServiceResponse> CreateAsync(int userId, ServiceRequest request);
    Task<ServiceResponse> UpdateAsync(int userId, int serviceId, ServiceRequest request);
    Task<bool> DeleteAsync(int userId, int serviceId);
    Task<bool> ToggleActiveAsync(int userId, int serviceId);
}
