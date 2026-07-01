using Domain.Common;

namespace Domain.Entities;

public class WorkerService : BaseEntity, ISoftDelete
{
    public int WorkerProfileId { get; set; }
    public WorkerProfile WorkerProfile { get; set; } = null!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string PriceType { get; set; } = "Fixed";
    public decimal Price { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public string? MaterialsIncluded { get; set; }
    public string? AvailableCities { get; set; }
    public string? Tags { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public ICollection<ServiceImage> Images { get; set; } = new List<ServiceImage>();
}

public class ServiceImage : BaseEntity
{
    public int WorkerServiceId { get; set; }
    public WorkerService WorkerService { get; set; } = null!;
    public string ImageUrl { get; set; } = string.Empty;
}
