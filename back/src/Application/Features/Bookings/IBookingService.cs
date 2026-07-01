using Application.Common.Models;

namespace Application.Features.Bookings;

public interface IBookingService
{
    Task<BookingResponse?> GetByIdAsync(int id);
    Task<PagedResult<BookingResponse>> GetCustomerBookingsAsync(int customerId, BookingFilterRequest filter);
    Task<PagedResult<BookingResponse>> GetWorkerBookingsAsync(int workerProfileId, BookingFilterRequest filter);
    Task<BookingResponse> CreateAsync(int customerId, CreateBookingRequest request);
    Task<BookingResponse> AcceptAsync(int workerUserId, int bookingId);
    Task<BookingResponse> RejectAsync(int workerUserId, int bookingId, string? reason = null);
    Task<BookingResponse> CancelAsync(int userId, int bookingId, string? reason = null);
    Task<BookingResponse> RescheduleAsync(int userId, int bookingId, DateTime newScheduledAt);
    Task<BookingResponse> MarkOnTheWayAsync(int workerUserId, int bookingId);
    Task<BookingResponse> StartJobAsync(int workerUserId, int bookingId);
    Task<BookingResponse> PauseJobAsync(int workerUserId, int bookingId);
    Task<BookingResponse> CompleteJobAsync(int workerUserId, int bookingId);
    Task AutoExpireBookingsAsync();
}
