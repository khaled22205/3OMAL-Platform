using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using src.DTOs.Favorites;
using src.Services.Interfaces;

namespace src.Controllers.V1;

[Authorize(Roles = "Customer")]
public class FavoritesController : BaseApiController
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        var favorites = await _favoriteService.GetUserFavoritesAsync(userId, page, pageSize);
        return OkResult(favorites);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddFavoriteRequest request)
    {
        var userId = GetUserId();
        var favorite = await _favoriteService.AddAsync(userId, request);
        return CreatedResult(favorite);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id)
    {
        var userId = GetUserId();
        var result = await _favoriteService.RemoveAsync(userId, id);
        if (!result) return NotFoundResult("Favorite not found");
        return OkResult(new { message = "Removed from favorites" });
    }
}