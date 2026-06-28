using src.DTOs.Admin;
using src.DTOs.Auth;
using src.DTOs.Bookings;
using src.DTOs.Categories;
using src.DTOs.Favorites;
using src.DTOs.Payments;
using src.DTOs.Reviews;
using src.DTOs.Services;
using src.DTOs.Workers;
using src.Models;
using Microsoft.AspNetCore.Identity;

namespace src.Helpers;

public static class MappingHelper
{
    public static UserInfo ToUserInfo(this IdentityUser<int> user, List<string> roles) => new()
    {
        Id = user.Id,
        Email = user.Email ?? "",
        PhoneNumber = user.PhoneNumber,
        Roles = roles
    };

    public static CategoryResponse ToResponse(this Category category, int servicesCount = 0) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
        Icon = category.Icon,
        Banner = category.Banner,
        SeoUrl = category.SeoUrl,
        ParentCategoryId = category.ParentCategoryId,
        ParentCategoryName = category.ParentCategory?.Name,
        SortOrder = category.SortOrder,
        IsActive = category.IsActive,
        ServicesCount = servicesCount,
        SubCategories = category.SubCategories?.Select(c => c.ToResponse()).ToList() ?? []
    };

    public static CategoryTreeResponse ToTreeResponse(this Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        SeoUrl = category.SeoUrl,
        SortOrder = category.SortOrder,
        Children = category.SubCategories?.Select(c => c.ToTreeResponse()).ToList() ?? []
    };

    public static WorkerProfileResponse ToResponse(this WorkerProfile profile, IdentityUser<int> user,
        List<WorkerAvailability>? availabilities = null, List<WorkerPortfolioItem>? portfolio = null) => new()
    {
        Id = profile.Id,
        UserId = profile.UserId,
        FirstName = user.UserName ?? "",
        LastName = "",
        Email = user.Email ?? "",
        PhoneNumber = user.PhoneNumber,
        Photo = profile.Photo,
        CoverPhoto = profile.CoverPhoto,
        Biography = profile.Biography,
        YearsOfExperience = profile.YearsOfExperience,
        Skills = profile.Skills,
        ServiceAreas = profile.ServiceAreas,
        HourlyRate = profile.HourlyRate,
        StartingPrice = profile.StartingPrice,
        CompletedJobs = profile.CompletedJobs,
        AverageRating = profile.AverageRating,
        IsAvailable = profile.IsAvailable,
        IsVerified = profile.IsVerified,
        Availability = availabilities?.Select(a => a.ToResponse()).ToList() ?? [],
        Portfolio = portfolio?.Select(p => p.ToResponse()).ToList() ?? []
    };

    public static WorkerAvailabilityResponse ToResponse(this WorkerAvailability availability) => new()
    {
        Id = availability.Id,
        DayOfWeek = availability.DayOfWeek.ToString(),
        StartTime = availability.StartTime.ToString(@"hh\:mm"),
        EndTime = availability.EndTime.ToString(@"hh\:mm"),
        IsAvailable = availability.IsAvailable
    };

    public static WorkerPortfolioResponse ToResponse(this WorkerPortfolioItem item) => new()
    {
        Id = item.Id,
        MediaType = item.MediaType,
        MediaUrl = item.MediaUrl,
        Title = item.Title
    };

    public static WorkerSummaryResponse ToSummary(this WorkerProfile profile, IdentityUser<int> user,
        List<string>? categories = null) => new()
    {
        Id = profile.Id,
        FirstName = user.UserName ?? "",
        LastName = "",
        Photo = profile.Photo,
        Biography = profile.Biography,
        YearsOfExperience = profile.YearsOfExperience,
        StartingPrice = profile.StartingPrice,
        AverageRating = profile.AverageRating,
        CompletedJobs = profile.CompletedJobs,
        IsAvailable = profile.IsAvailable,
        IsVerified = profile.IsVerified,
        ServiceAreas = profile.ServiceAreas,
        Categories = categories ?? []
    };

    public static ServiceResponse ToResponse(this WorkerService service, string workerName = "",
        string categoryName = "", List<string>? images = null) => new()
    {
        Id = service.Id,
        WorkerProfileId = service.WorkerProfileId,
        WorkerName = workerName,
        CategoryId = service.CategoryId,
        CategoryName = categoryName,
        Title = service.Title,
        Description = service.Description,
        PriceType = service.PriceType,
        Price = service.Price,
        EstimatedDurationMinutes = service.EstimatedDurationMinutes,
        MaterialsIncluded = service.MaterialsIncluded,
        AvailableCities = service.AvailableCities,
        Tags = service.Tags,
        IsActive = service.IsActive,
        Images = images ?? [],
        CreatedAt = service.CreatedAt
    };

    public static BookingResponse ToResponse(this Booking booking, string customerName = "",
        string workerName = "", string? serviceName = null) => new()
    {
        Id = booking.Id,
        CustomerId = booking.CustomerId,
        CustomerName = customerName,
        WorkerProfileId = booking.WorkerProfileId,
        WorkerName = workerName,
        WorkerServiceId = booking.WorkerServiceId,
        ServiceName = serviceName,
        Status = booking.Status,
        ScheduledAt = booking.ScheduledAt,
        Address = booking.Address,
        Notes = booking.Notes,
        TotalPrice = booking.TotalPrice,
        CommissionAmount = booking.CommissionAmount,
        CreatedAt = booking.CreatedAt,
        StartedAt = booking.StartedAt,
        CompletedAt = booking.CompletedAt,
        CancelledAt = booking.CancelledAt,
        CancellationReason = booking.CancellationReason
    };

    public static ReviewResponse ToResponse(this Review review, string customerName = "",
        string? customerPhoto = null, string workerName = "") => new()
    {
        Id = review.Id,
        BookingId = review.BookingId,
        CustomerId = review.CustomerId,
        CustomerName = customerName,
        CustomerPhoto = customerPhoto,
        WorkerProfileId = review.WorkerProfileId,
        WorkerName = workerName,
        Rating = review.Rating,
        Comment = review.Comment,
        WorkerReply = review.WorkerReply,
        IsEdited = review.IsEdited,
        CreatedAt = review.CreatedAt
    };

    public static PaymentResponse ToResponse(this Payment payment) => new()
    {
        Id = payment.Id,
        BookingId = payment.BookingId,
        Amount = payment.Amount,
        CommissionAmount = payment.CommissionAmount,
        PaymentMethod = payment.PaymentMethod,
        Status = payment.Status,
        CompletedAt = payment.CompletedAt,
        TransactionReference = payment.TransactionReference
    };

    public static FavoriteResponse ToResponse(this Favorite favorite, string? workerName = null,
        string? workerPhoto = null, double? workerRating = null,
        string? serviceName = null, decimal? servicePrice = null) => new()
    {
        Id = favorite.Id,
        WorkerProfileId = favorite.WorkerProfileId,
        WorkerName = workerName,
        WorkerPhoto = workerPhoto,
        WorkerRating = workerRating,
        WorkerServiceId = favorite.WorkerServiceId,
        ServiceName = serviceName,
        ServicePrice = servicePrice,
        CreatedAt = favorite.CreatedAt
    };

    public static UserManagementResponse ToManagementResponse(this IdentityUser<int> user, List<string> roles) => new()
    {
        Id = user.Id,
        Email = user.Email ?? "",
        PhoneNumber = user.PhoneNumber,
        UserName = user.UserName ?? "",
        Roles = roles,
        EmailConfirmed = user.EmailConfirmed,
        PhoneNumberConfirmed = user.PhoneNumberConfirmed,
        IsLockedOut = user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
        LockoutEnd = user.LockoutEnd?.UtcDateTime,
        CreatedAt = new DateTime(1970, 1, 1).AddSeconds(0)
    };
}