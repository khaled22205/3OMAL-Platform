using Application.Features.Auth;
using Application.Features.Bookings;
using Application.Features.Categories;
using Application.Features.Reviews;
using Application.Features.Services;
using Application.Features.Workers;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void Should_pass_for_valid_request()
    {
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "password123",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+1234567890",
            UserType = "Customer"
        };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Should_fail_when_email_is_invalid(string email)
    {
        var request = new RegisterRequest { Email = email, Password = "password123", FirstName = "John", LastName = "Doe", PhoneNumber = "+1234567890", UserType = "Customer" };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    public void Should_fail_when_password_is_invalid(string password)
    {
        var request = new RegisterRequest { Email = "test@example.com", Password = password, FirstName = "John", LastName = "Doe", PhoneNumber = "+1234567890", UserType = "Customer" };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_fail_when_firstName_is_empty()
    {
        var request = new RegisterRequest { Email = "test@example.com", Password = "password123", FirstName = "", LastName = "Doe", PhoneNumber = "+1234567890", UserType = "Customer" };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_fail_when_userType_is_invalid()
    {
        var request = new RegisterRequest { Email = "test@example.com", Password = "password123", FirstName = "John", LastName = "Doe", PhoneNumber = "+1234567890", UserType = "Invalid" };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Customer") && e.ErrorMessage.Contains("Worker"));
    }
}

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Should_pass_for_valid_request()
    {
        var request = new LoginRequest { Email = "test@example.com", Password = "password123" };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_fail_when_email_is_empty()
    {
        var request = new LoginRequest { Email = "", Password = "password123" };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_fail_when_password_is_empty()
    {
        var request = new LoginRequest { Email = "test@example.com", Password = "" };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }
}

public class CategoryRequestValidatorTests
{
    private readonly CategoryRequestValidator _validator = new();

    [Fact]
    public void Should_pass_for_valid_request()
    {
        var request = new CategoryRequest { Name = "Plumbing", SeoUrl = "plumbing" };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_fail_when_name_is_empty()
    {
        var request = new CategoryRequest { Name = "" };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_fail_when_name_exceeds_maximum_length()
    {
        var request = new CategoryRequest { Name = new string('x', 101) };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }
}

public class WorkerProfileRequestValidatorTests
{
    private readonly WorkerProfileRequestValidator _validator = new();

    [Fact]
    public void Should_pass_for_valid_request()
    {
        var request = new WorkerProfileRequest { Biography = "Experienced plumber", YearsOfExperience = 5, HourlyRate = 50, StartingPrice = 100 };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_fail_when_biography_exceeds_maximum_length()
    {
        var request = new WorkerProfileRequest { Biography = new string('x', 2001), YearsOfExperience = 5, HourlyRate = 50, StartingPrice = 100 };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(71)]
    public void Should_fail_when_yearsOfExperience_is_out_of_range(int years)
    {
        var request = new WorkerProfileRequest { YearsOfExperience = years, HourlyRate = 50, StartingPrice = 100 };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_fail_when_hourlyRate_is_negative()
    {
        var request = new WorkerProfileRequest { HourlyRate = -1, StartingPrice = 100 };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_fail_when_startingPrice_is_negative()
    {
        var request = new WorkerProfileRequest { StartingPrice = -1, HourlyRate = 50 };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }
}

public class ServiceRequestValidatorTests
{
    private readonly ServiceRequestValidator _validator = new();

    [Fact]
    public void Should_pass_for_valid_request()
    {
        var request = new ServiceRequest { Title = "Fix leaky pipe", PriceType = "Fixed", Price = 150, EstimatedDurationMinutes = 60, CategoryId = 1 };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_fail_when_title_is_empty()
    {
        var request = new ServiceRequest { Title = "", PriceType = "Fixed", Price = 150, EstimatedDurationMinutes = 60, CategoryId = 1 };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_fail_when_title_exceeds_maximum_length()
    {
        var request = new ServiceRequest { Title = new string('x', 201), PriceType = "Fixed", Price = 150, EstimatedDurationMinutes = 60, CategoryId = 1 };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Invalid")]
    public void Should_fail_when_priceType_is_invalid(string priceType)
    {
        var request = new ServiceRequest { Title = "Fix leaky pipe", PriceType = priceType, Price = 150, EstimatedDurationMinutes = 60, CategoryId = 1 };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_fail_when_price_is_zero_or_negative()
    {
        var request = new ServiceRequest { Title = "Fix leaky pipe", PriceType = "Fixed", Price = 0, EstimatedDurationMinutes = 60, CategoryId = 1 };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_fail_when_estimatedDuration_is_zero_or_negative()
    {
        var request = new ServiceRequest { Title = "Fix leaky pipe", PriceType = "Fixed", Price = 150, EstimatedDurationMinutes = 0, CategoryId = 1 };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_fail_when_categoryId_is_zero_or_negative()
    {
        var request = new ServiceRequest { Title = "Fix leaky pipe", PriceType = "Fixed", Price = 150, EstimatedDurationMinutes = 60, CategoryId = 0 };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }
}

public class CreateBookingRequestValidatorTests
{
    private readonly CreateBookingRequestValidator _validator = new();

    [Fact]
    public void Should_pass_for_valid_request()
    {
        var request = new CreateBookingRequest { WorkerProfileId = 1, ScheduledAt = DateTime.UtcNow.AddDays(1), Address = "123 Main St" };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_fail_when_workerProfileId_is_zero()
    {
        var request = new CreateBookingRequest { WorkerProfileId = 0, ScheduledAt = DateTime.UtcNow.AddDays(1), Address = "123 Main St" };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_fail_when_scheduledAt_is_in_the_past()
    {
        var request = new CreateBookingRequest { WorkerProfileId = 1, ScheduledAt = DateTime.UtcNow.AddDays(-1), Address = "123 Main St" };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_fail_when_address_is_empty()
    {
        var request = new CreateBookingRequest { WorkerProfileId = 1, ScheduledAt = DateTime.UtcNow.AddDays(1), Address = "" };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_fail_when_address_exceeds_maximum_length()
    {
        var request = new CreateBookingRequest { WorkerProfileId = 1, ScheduledAt = DateTime.UtcNow.AddDays(1), Address = new string('x', 501) };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }
}

public class CreateReviewRequestValidatorTests
{
    private readonly CreateReviewRequestValidator _validator = new();

    [Fact]
    public void Should_pass_for_valid_request()
    {
        var request = new CreateReviewRequest { BookingId = 1, Rating = 5, Comment = "Great work!" };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_fail_when_bookingId_is_zero()
    {
        var request = new CreateReviewRequest { BookingId = 0, Rating = 5 };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Should_fail_when_rating_is_out_of_range(int rating)
    {
        var request = new CreateReviewRequest { BookingId = 1, Rating = rating };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_pass_when_rating_is_at_boundary()
    {
        var request = new CreateReviewRequest { BookingId = 1, Rating = 1 };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_fail_when_comment_exceeds_maximum_length()
    {
        var request = new CreateReviewRequest { BookingId = 1, Rating = 5, Comment = new string('x', 2001) };
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }
}
