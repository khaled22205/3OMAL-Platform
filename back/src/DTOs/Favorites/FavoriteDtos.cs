namespace src.DTOs.Favorites;

public class AddFavoriteRequest
{
    public int? WorkerProfileId { get; set; }
    public int? WorkerServiceId { get; set; }
}

public class FavoriteResponse
{
    public int Id { get; set; }
    public int? WorkerProfileId { get; set; }
    public string? WorkerName { get; set; }
    public string? WorkerPhoto { get; set; }
    public double? WorkerRating { get; set; }
    public int? WorkerServiceId { get; set; }
    public string? ServiceName { get; set; }
    public decimal? ServicePrice { get; set; }
    public DateTime CreatedAt { get; set; }
}