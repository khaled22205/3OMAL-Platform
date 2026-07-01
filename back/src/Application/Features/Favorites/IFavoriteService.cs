using Application.Common.Models;

namespace Application.Features.Favorites;

public interface IFavoriteService
{
    Task<PagedResult<FavoriteResponse>> GetUserFavoritesAsync(int customerId, int page, int pageSize);
    Task<FavoriteResponse> AddAsync(int customerId, AddFavoriteRequest request);
    Task<bool> RemoveAsync(int customerId, int favoriteId);
    Task<bool> IsFavoritedAsync(int customerId, int? workerProfileId, int? serviceId);
}
