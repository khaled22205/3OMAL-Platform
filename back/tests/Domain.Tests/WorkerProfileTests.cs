using Domain.Entities;
using FluentAssertions;

namespace Domain.Tests;

public class WorkerProfileTests
{
    private static WorkerProfile CreateProfile() => new()
    {
        UserId = 1,
        HourlyRate = 100,
        StartingPrice = 200,
        YearsOfExperience = 5
    };

    public class IncrementCompletedJobs
    {
        [Fact]
        public void Should_increment_by_one()
        {
            var profile = CreateProfile();
            profile.CompletedJobs.Should().Be(0);
            profile.IncrementCompletedJobs();
            profile.CompletedJobs.Should().Be(1);
            profile.IncrementCompletedJobs();
            profile.CompletedJobs.Should().Be(2);
        }
    }

    public class UpdateRating
    {
        [Fact]
        public void Should_round_to_one_decimal()
        {
            var profile = CreateProfile();
            profile.UpdateRating(4.56789);
            profile.AverageRating.Should().Be(4.6);
        }

        [Fact]
        public void Should_accept_zero()
        {
            var profile = CreateProfile();
            profile.UpdateRating(0);
            profile.AverageRating.Should().Be(0);
        }

        [Fact]
        public void Should_accept_five()
        {
            var profile = CreateProfile();
            profile.UpdateRating(5.0);
            profile.AverageRating.Should().Be(5.0);
        }
    }

    public class Defaults
    {
        [Fact]
        public void Should_be_available_by_default()
        {
            var profile = new WorkerProfile();
            profile.IsAvailable.Should().BeTrue();
        }

        [Fact]
        public void Should_not_be_verified_by_default()
        {
            var profile = new WorkerProfile();
            profile.IsVerified.Should().BeFalse();
        }

        [Fact]
        public void Should_have_zero_completed_jobs_by_default()
        {
            var profile = new WorkerProfile();
            profile.CompletedJobs.Should().Be(0);
        }
    }
}
