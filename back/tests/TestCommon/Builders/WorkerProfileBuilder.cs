using Domain.Entities;

namespace TestCommon.Builders;

public class WorkerProfileBuilder
{
    private int _userId = 2;
    private string? _biography = "Experienced worker";
    private int _yearsOfExperience = 5;
    private decimal _hourlyRate = 50m;
    private decimal _startingPrice = 100m;
    private bool _isAvailable = true;
    private bool _isVerified;
    private int _completedJobs;
    private double _averageRating;
    private string? _skills = "skill1, skill2";
    private string? _serviceAreas = "Area1, Area2";

    public WorkerProfileBuilder WithUserId(int id) { _userId = id; return this; }
    public WorkerProfileBuilder WithBiography(string? bio) { _biography = bio; return this; }
    public WorkerProfileBuilder WithYearsOfExperience(int yrs) { _yearsOfExperience = yrs; return this; }
    public WorkerProfileBuilder WithHourlyRate(decimal rate) { _hourlyRate = rate; return this; }
    public WorkerProfileBuilder WithStartingPrice(decimal price) { _startingPrice = price; return this; }
    public WorkerProfileBuilder NotAvailable() { _isAvailable = false; return this; }
    public WorkerProfileBuilder Verified() { _isVerified = true; return this; }
    public WorkerProfileBuilder WithCompletedJobs(int jobs) { _completedJobs = jobs; return this; }
    public WorkerProfileBuilder WithAverageRating(double rating) { _averageRating = rating; return this; }

    public WorkerProfile Build()
    {
        return new WorkerProfile
        {
            UserId = _userId,
            Biography = _biography,
            YearsOfExperience = _yearsOfExperience,
            HourlyRate = _hourlyRate,
            StartingPrice = _startingPrice,
            IsAvailable = _isAvailable,
            IsVerified = _isVerified,
            CompletedJobs = _completedJobs,
            AverageRating = _averageRating,
            Skills = _skills,
            ServiceAreas = _serviceAreas,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static WorkerProfile CreateDefault(int userId = 2)
        => new WorkerProfileBuilder().WithUserId(userId).Build();
}
