using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Bookings;
using Application.Common.Mappings;
using Domain.DomainServices;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;

namespace Infrastructure.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;
    private readonly IIdentityService _identity;
    private readonly IConfiguration _configuration;

    public BookingService(AppDbContext context, IIdentityService identity, IConfiguration configuration)
    {
        _context = context;
        _identity = identity;
        _configuration = configuration;
    }

    public async Task<BookingResponse?> GetByIdAsync(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.WorkerProfile)
            .Include(b => b.WorkerService)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return null;

        var customerName = await _identity.GetUserNameAsync(booking.CustomerId) ?? "";
        var workerName = await _identity.GetUserNameAsync(booking.WorkerProfile.UserId) ?? "";

        return booking.ToResponse(customerName, workerName, booking.WorkerService?.Title);
    }

    public async Task<PagedResult<BookingResponse>> GetCustomerBookingsAsync(int customerId, BookingFilterRequest filter)
    {
        var query = _context.Bookings
            .Include(b => b.WorkerProfile)
            .Include(b => b.WorkerService)
            .Where(b => b.CustomerId == customerId);

        query = ApplyFilter(query, filter);

        return await PaginateBookingsAsync(query, filter.Page, filter.PageSize);
    }

    public async Task<PagedResult<BookingResponse>> GetWorkerBookingsAsync(int workerProfileId, BookingFilterRequest filter)
    {
        var query = _context.Bookings
            .Include(b => b.WorkerProfile)
            .Include(b => b.WorkerService)
            .Where(b => b.WorkerProfileId == workerProfileId);

        query = ApplyFilter(query, filter);

        return await PaginateBookingsAsync(query, filter.Page, filter.PageSize);
    }

    public async Task<BookingResponse> CreateAsync(int customerId, CreateBookingRequest request)
    {
        var workerProfile = await _context.WorkerProfiles.FindAsync(request.WorkerProfileId)
            ?? throw new ArgumentException("Worker not found");

        if (!workerProfile.IsAvailable)
            throw new InvalidOperationException("Worker is not available");

        decimal price = workerProfile.StartingPrice;
        if (request.WorkerServiceId.HasValue)
        {
            var service = await _context.WorkerServices.FindAsync(request.WorkerServiceId.Value)
                ?? throw new ArgumentException("Service not found");
            price = service.Price;
        }

        var commissionPercentage = _configuration.GetSection("Commission").GetValue<double>("Percentage");
        var commissionAmount = CommissionCalculator.Calculate(price, commissionPercentage);

        var booking = new Booking
        {
            CustomerId = customerId,
            WorkerProfileId = request.WorkerProfileId,
            WorkerServiceId = request.WorkerServiceId,
            ScheduledAt = request.ScheduledAt,
            Address = request.Address,
            Notes = request.Notes,
            TotalPrice = price,
            CommissionAmount = commissionAmount
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(booking.Id))!;
    }

    public async Task<BookingResponse> AcceptAsync(int workerUserId, int bookingId)
    {
        var booking = await GetWorkerBookingAsync(workerUserId, bookingId);
        booking.Accept();
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(bookingId))!;
    }

    public async Task<BookingResponse> RejectAsync(int workerUserId, int bookingId, string? reason = null)
    {
        var booking = await GetWorkerBookingAsync(workerUserId, bookingId);
        booking.Reject(reason);
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(bookingId))!;
    }

    public async Task<BookingResponse> CancelAsync(int userId, int bookingId, string? reason = null)
    {
        var booking = await _context.Bookings.FindAsync(bookingId)
            ?? throw new KeyNotFoundException("Booking not found");

        if (booking.CustomerId != userId)
        {
            var workerProfile = await _context.WorkerProfiles
                .FirstOrDefaultAsync(w => w.UserId == userId);
            if (workerProfile == null || booking.WorkerProfileId != workerProfile.Id)
                throw new UnauthorizedAccessException("Not authorized to cancel this booking");
        }

        booking.Cancel(userId, reason);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(bookingId))!;
    }

    public async Task<BookingResponse> RescheduleAsync(int userId, int bookingId, DateTime newScheduledAt)
    {
        var booking = await _context.Bookings.FindAsync(bookingId)
            ?? throw new KeyNotFoundException("Booking not found");

        if (booking.CustomerId != userId)
        {
            var workerProfile = await _context.WorkerProfiles
                .FirstOrDefaultAsync(w => w.UserId == userId);
            if (workerProfile == null || booking.WorkerProfileId != workerProfile.Id)
                throw new UnauthorizedAccessException("Not authorized to reschedule");
        }

        booking.Reschedule(newScheduledAt);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(bookingId))!;
    }

    public async Task<BookingResponse> MarkOnTheWayAsync(int workerUserId, int bookingId)
    {
        var booking = await GetWorkerBookingAsync(workerUserId, bookingId);
        booking.MarkOnTheWay();
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(bookingId))!;
    }

    public async Task<BookingResponse> StartJobAsync(int workerUserId, int bookingId)
    {
        var booking = await GetWorkerBookingAsync(workerUserId, bookingId);
        booking.StartJob();
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(bookingId))!;
    }

    public async Task<BookingResponse> PauseJobAsync(int workerUserId, int bookingId)
    {
        var booking = await GetWorkerBookingAsync(workerUserId, bookingId);
        booking.PauseJob();
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(bookingId))!;
    }

    public async Task<BookingResponse> CompleteJobAsync(int workerUserId, int bookingId)
    {
        var booking = await GetWorkerBookingAsync(workerUserId, bookingId);
        booking.CompleteJob();

        var worker = await _context.WorkerProfiles.FindAsync(booking.WorkerProfileId);
        if (worker != null)
        {
            worker.IncrementCompletedJobs();
            worker.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        await GenerateInvoice(booking);

        return (await GetByIdAsync(bookingId))!;
    }

    public async Task AutoExpireBookingsAsync()
    {
        var expiryMinutes = _configuration.GetSection("Booking").GetValue<int>("AutoExpiryMinutes");
        var cutoff = DateTime.UtcNow.AddMinutes(-expiryMinutes);

        var expired = await _context.Bookings
            .Where(b => b.Status == BookingStatus.Pending && b.CreatedAt <= cutoff)
            .ToListAsync();

        foreach (var booking in expired)
            booking.Expire();

        await _context.SaveChangesAsync();
    }

    private async Task<Booking> GetWorkerBookingAsync(int workerUserId, int bookingId)
    {
        var profile = await _context.WorkerProfiles
            .FirstOrDefaultAsync(w => w.UserId == workerUserId)
            ?? throw new InvalidOperationException("Worker profile not found");

        return await _context.Bookings
                   .FirstOrDefaultAsync(b => b.Id == bookingId && b.WorkerProfileId == profile.Id)
               ?? throw new KeyNotFoundException("Booking not found");
    }

    private IQueryable<Booking> ApplyFilter(IQueryable<Booking> query, BookingFilterRequest filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(b => b.Status.ToString() == filter.Status);
        if (filter.FromDate.HasValue)
            query = query.Where(b => b.ScheduledAt >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            query = query.Where(b => b.ScheduledAt <= filter.ToDate.Value);

        return query.OrderByDescending(b => b.CreatedAt);
    }

    private async Task<PagedResult<BookingResponse>> PaginateBookingsAsync(IQueryable<Booking> query, int page, int pageSize)
    {
        var totalCount = await query.CountAsync();
        var bookings = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = new List<BookingResponse>();
        foreach (var b in bookings)
        {
            var customerName = await _identity.GetUserNameAsync(b.CustomerId) ?? "";
            var workerName = await _identity.GetUserNameAsync(b.WorkerProfile.UserId) ?? "";
            items.Add(b.ToResponse(customerName, workerName, b.WorkerService?.Title));
        }

        return new PagedResult<BookingResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private async Task GenerateInvoice(Booking booking)
    {
        var invoice = new Invoice
        {
            BookingId = booking.Id,
            InvoiceNumber = StringHelper.GenerateInvoiceNumber(booking.Id),
            Amount = booking.TotalPrice,
            CommissionAmount = booking.CommissionAmount,
            WorkerAmount = booking.TotalPrice - booking.CommissionAmount
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
    }
}
