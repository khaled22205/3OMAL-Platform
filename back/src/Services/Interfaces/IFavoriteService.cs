using src.DTOs.Common;
using src.DTOs.Favorites;

namespace src.Services.Interfaces;

public interface IFavoriteService
{
    Task<PagedResponse<FavoriteResponse>> GetUserFavoritesAsync(int customerId, int page, int pageSize);
    Task<FavoriteResponse> AddAsync(int customerId, AddFavoriteRequest request);
    Task<bool> RemoveAsync(int customerId, int favoriteId);
    Task<bool> IsFavoritedAsync(int customerId, int? workerProfileId, int? serviceId);
}