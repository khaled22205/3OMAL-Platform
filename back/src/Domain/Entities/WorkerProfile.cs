using Domain.Common;

namespace Domain.Entities;

public class WorkerProfile : BaseEntity, ISoftDelete
{
    public int UserId { get; set; }
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
    public bool IsAvailable { get; set; } = true;
    public decimal? MinimumJobValue { get; set; }
    public bool IsVerified { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public void IncrementCompletedJobs()
    {
        CompletedJobs++;
    }

    public void UpdateRating(double newAverage)
    {
        AverageRating = Math.Round(newAverage, 1);
    }
}
