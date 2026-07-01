namespace Application.Features.Workers;

public class WorkerProfileRequest
{
    public string? Photo { get; set; }
    public string? CoverPhoto { get; set; }
    public string? Biography { get; set; }
    public int YearsOfExperience { get; set; }
    public string? Skills { get; set; }
    public string? ServiceAreas { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal StartingPrice { get; set; }
    public decimal? MinimumJobValue { get; set; }
}

public class WorkerProfileResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Photo { get; set; }
    public string? CoverPhoto { get; set; }
    public string? Biography { get; set; }
    public int YearsOfExperience { get; set; }
    public string? Skills { get; set; }
    public string? ServiceAreas { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal StartingPrice { get; set; }
    public int CompletedJobs { get; set; }
    public double AverageRating { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsVerified { get; set; }
    public List<WorkerAvailabilityResponse> Availability { get; set; } = [];
    public List<WorkerPortfolioResponse> Portfolio { get; set; } = [];
}

public class WorkerAvailabilityRequest
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;
}

public class WorkerAvailabilityResponse
{
    public int Id { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}

public class WorkerPortfolioResponse
{
    public int Id { get; set; }
    public string MediaType { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
    public string? Title { get; set; }
}

public class WorkerSearchRequest
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public double? MinRating { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? City { get; set; }
    public string? Area { get; set; }
    public int? MinExperience { get; set; }
    public bool? AvailableNow { get; set; }
    public string? SortBy { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class WorkerSummaryResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Photo { get; set; }
    public string? Biography { get; set; }
    public int YearsOfExperience { get; set; }
    public decimal StartingPrice { get; set; }
    public double AverageRating { get; set; }
    public int CompletedJobs { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsVerified { get; set; }
    public string? ServiceAreas { get; set; }
    public List<string> Categories { get; set; } = [];
}

public class WorkerStatusRequest
{
    public bool IsAvailable { get; set; }
}

public class WorkerPortfolioRequest
{
    public string MediaType { get; set; } = "Image";
    public string MediaUrl { get; set; } = string.Empty;
    public string? Title { get; set; }
}
