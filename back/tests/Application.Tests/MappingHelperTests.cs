using Application.Common.Mappings;
using Application.Features.Auth;
using Application.Features.Categories;
using Application.Features.Workers;
using Application.Features.Services;
using Application.Features.Bookings;
using Application.Features.Reviews;
using Application.Features.Payments;
using Application.Features.Favorites;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;

namespace Application.Tests;

public class ToUserInfoTests
{
    [Fact]
    public void Should_map_tuple_to_UserInfo()
    {
        var result = (1, "test@test.com", "1234567890", new List<string> { "Customer" }).ToUserInfo();

        result.Id.Should().Be(1);
        result.Email.Should().Be("test@test.com");
        result.PhoneNumber.Should().Be("1234567890");
        result.Roles.Should().ContainSingle("Customer");
    }

    [Fact]
    public void Should_handle_null_phone()
    {
        var result = (1, "test@test.com", (string?)null, new List<string>()).ToUserInfo();
        result.PhoneNumber.Should().BeNull();
    }
}

public class CategoryMappingTests
{
    [Fact]
    public void ToResponse_Should_map_all_properties()
    {
        var parent = new Category { Id = 10, Name = "Parent" };
        var category = new Category
        {
            Id = 1,
            Name = "Plumbing",
            Description = "All plumbing services",
            Icon = "icon.png",
            Banner = "banner.png",
            SeoUrl = "plumbing",
            ParentCategoryId = 10,
            ParentCategory = parent,
            SortOrder = 1,
            IsActive = true,
            SubCategories = new List<Category>
            {
                new() { Id = 2, Name = "Pipe Repair" }
            }
        };

        var result = category.ToResponse(5);

        result.Id.Should().Be(1);
        result.Name.Should().Be("Plumbing");
        result.ServicesCount.Should().Be(5);
        result.ParentCategoryName.Should().Be("Parent");
        result.SubCategories.Should().ContainSingle().Which.Name.Should().Be("Pipe Repair");
    }

    [Fact]
    public void ToResponse_Should_handle_no_subcategories()
    {
        var category = new Category { Id = 1, Name = "Plumbing" };
        var result = category.ToResponse();
        result.SubCategories.Should().BeEmpty();
    }

    [Fact]
    public void ToTreeResponse_Should_map_hierarchy()
    {
        var category = new Category
        {
            Id = 1,
            Name = "Home Services",
            SeoUrl = "home-services",
            SortOrder = 0,
            SubCategories = new List<Category>
            {
                new() { Id = 2, Name = "Plumbing", SeoUrl = "plumbing", SortOrder = 1 }
            }
        };

        var result = category.ToTreeResponse();
        result.Name.Should().Be("Home Services");
        result.Children.Should().ContainSingle().Which.Name.Should().Be("Plumbing");
    }
}

public class WorkerProfileMappingTests
{
    [Fact]
    public void ToResponse_Should_map_all_properties()
    {
        var profile = new WorkerProfile
        {
            Id = 1,
            UserId = 10,
            Photo = "photo.jpg",
            CoverPhoto = "cover.jpg",
            Biography = "Experienced",
            YearsOfExperience = 5,
            Skills = "Plumbing",
            ServiceAreas = "Cairo",
            HourlyRate = 50,
            StartingPrice = 100,
            CompletedJobs = 20,
            AverageRating = 4.5,
            IsAvailable = true,
            IsVerified = true
        };

        var availability = new List<WorkerAvailabilityResponse>
        {
            new() { Id = 1, DayOfWeek = "Monday", StartTime = "09:00", EndTime = "17:00", IsAvailable = true }
        };
        var portfolio = new List<WorkerPortfolioResponse>
        {
            new() { Id = 1, MediaType = "Image", MediaUrl = "img.jpg", Title = "My Work" }
        };

        var result = profile.ToResponse("John", "Doe", "john@test.com", "1234567890", availability, portfolio);

        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john@test.com");
        result.PhoneNumber.Should().Be("1234567890");
        result.Availability.Should().ContainSingle();
        result.Portfolio.Should().ContainSingle();
    }

    [Fact]
    public void ToSummary_Should_map_worker_summary()
    {
        var profile = new WorkerProfile
        {
            Id = 1,
            Photo = "photo.jpg",
            Biography = "Experienced",
            YearsOfExperience = 5,
            StartingPrice = 100,
            AverageRating = 4.5,
            CompletedJobs = 20,
            IsAvailable = true,
            IsVerified = true,
            ServiceAreas = "Cairo"
        };

        var result = profile.ToSummary("John", "Doe", new List<string> { "Plumbing" });

        result.FirstName.Should().Be("John");
        result.Categories.Should().ContainSingle("Plumbing");
    }
}

public class WorkerAvailabilityMappingTests
{
    [Fact]
    public void ToResponse_Should_map_availability()
    {
        var availability = new WorkerAvailability
        {
            Id = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            IsAvailable = true
        };

        var result = availability.ToResponse();

        result.DayOfWeek.Should().Be("Monday");
        result.StartTime.Should().Be("09:00");
        result.EndTime.Should().Be("17:00");
        result.IsAvailable.Should().BeTrue();
    }
}

public class WorkerPortfolioMappingTests
{
    [Fact]
    public void ToResponse_Should_map_portfolio()
    {
        var item = new WorkerPortfolioItem
        {
            Id = 1,
            MediaType = "Image",
            MediaUrl = "https://example.com/img.jpg",
            Title = "My Project"
        };

        var result = item.ToResponse();

        result.MediaType.Should().Be("Image");
        result.MediaUrl.Should().Be("https://example.com/img.jpg");
        result.Title.Should().Be("My Project");
    }
}

public class ServiceMappingTests
{
    [Fact]
    public void ToResponse_Should_map_service()
    {
        var service = new WorkerService
        {
            Id = 1,
            WorkerProfileId = 10,
            CategoryId = 5,
            Title = "Fix Leaky Pipe",
            Description = "Professional pipe repair",
            PriceType = "Fixed",
            Price = 150,
            EstimatedDurationMinutes = 60,
            MaterialsIncluded = "All materials",
            AvailableCities = "Cairo,Giza",
            Tags = "plumbing,repair",
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1)
        };

        var result = service.ToResponse("John Doe", "Plumbing", new List<string> { "img1.jpg" });

        result.WorkerName.Should().Be("John Doe");
        result.CategoryName.Should().Be("Plumbing");
        result.Images.Should().ContainSingle("img1.jpg");
        result.Title.Should().Be("Fix Leaky Pipe");
    }
}

public class BookingMappingTests
{
    [Fact]
    public void ToResponse_Should_map_booking()
    {
        var booking = new Booking
        {
            Id = 1,
            CustomerId = 100,
            WorkerProfileId = 10,
            WorkerServiceId = 5,
            Status = BookingStatus.Pending,
            ScheduledAt = new DateTime(2024, 6, 15, 10, 0, 0),
            Address = "123 Main St",
            Notes = "Please be on time",
            TotalPrice = 500,
            CommissionAmount = 50,
            CreatedAt = new DateTime(2024, 6, 10),
            StartedAt = null,
            CompletedAt = null,
            CancelledAt = null,
            CancellationReason = null
        };

        var result = booking.ToResponse("Jane Smith", "John Doe", "Fix Pipe");

        result.CustomerName.Should().Be("Jane Smith");
        result.WorkerName.Should().Be("John Doe");
        result.ServiceName.Should().Be("Fix Pipe");
        result.Status.Should().Be("Pending");
    }

    [Fact]
    public void ToResponse_Should_handle_nullable_fields()
    {
        var booking = new Booking
        {
            Id = 1,
            CustomerId = 100,
            WorkerProfileId = 10,
            Status = BookingStatus.Cancelled,
            ScheduledAt = DateTime.UtcNow,
            TotalPrice = 500,
            CommissionAmount = 50,
            CreatedAt = DateTime.UtcNow,
            CancelledAt = DateTime.UtcNow,
            CancellationReason = "No longer needed"
        };

        var result = booking.ToResponse();

        result.Status.Should().Be("Cancelled");
        result.CancellationReason.Should().Be("No longer needed");
    }
}

public class ReviewMappingTests
{
    [Fact]
    public void ToResponse_Should_map_review()
    {
        var review = new Review
        {
            Id = 1,
            BookingId = 100,
            CustomerId = 10,
            WorkerProfileId = 20,
            Rating = 5,
            Comment = "Excellent!",
            WorkerReply = "Thank you!",
            IsEdited = false,
            CreatedAt = new DateTime(2024, 6, 10)
        };

        var result = review.ToResponse("Jane Smith", "photo.jpg", "John Doe");

        result.CustomerName.Should().Be("Jane Smith");
        result.CustomerPhoto.Should().Be("photo.jpg");
        result.WorkerName.Should().Be("John Doe");
        result.Rating.Should().Be(5);
        result.WorkerReply.Should().Be("Thank you!");
    }
}

public class PaymentMappingTests
{
    [Fact]
    public void ToResponse_Should_map_payment()
    {
        var payment = new Payment
        {
            Id = 1,
            BookingId = 100,
            Amount = 500,
            CommissionAmount = 50,
            PaymentMethod = "CreditCard",
            Status = "Completed",
            CompletedAt = new DateTime(2024, 6, 10),
            TransactionReference = "TXN123"
        };

        var result = payment.ToResponse();

        result.Amount.Should().Be(500);
        result.CommissionAmount.Should().Be(50);
        result.PaymentMethod.Should().Be("CreditCard");
        result.Status.Should().Be("Completed");
        result.TransactionReference.Should().Be("TXN123");
    }
}

public class FavoriteMappingTests
{
    [Fact]
    public void ToResponse_Should_map_favorite()
    {
        var favorite = new Favorite
        {
            Id = 1,
            WorkerProfileId = 10,
            WorkerServiceId = 5,
            CreatedAt = new DateTime(2024, 6, 10)
        };

        var result = favorite.ToResponse("John Doe", "photo.jpg", 4.5, "Fix Pipe", 150m);

        result.WorkerName.Should().Be("John Doe");
        result.WorkerPhoto.Should().Be("photo.jpg");
        result.WorkerRating.Should().Be(4.5);
        result.ServiceName.Should().Be("Fix Pipe");
        result.ServicePrice.Should().Be(150);
    }
}
