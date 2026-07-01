namespace Application.Features.Services;

public class ServiceRequest
{
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string PriceType { get; set; } = "Fixed";
    public decimal Price { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public string? MaterialsIncluded { get; set; }
    public string? AvailableCities { get; set; }
    public string? Tags { get; set; }
    public List<string> Images { get; set; } = [];
}

public class ServiceResponse
{
    public int Id { get; set; }
    public int WorkerProfileId { get; set; }
    public string WorkerName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string PriceType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public string? MaterialsIncluded { get; set; }
    public string? AvailableCities { get; set; }
    public string? Tags { get; set; }
    public bool IsActive { get; set; }
    public List<string> Images { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}
