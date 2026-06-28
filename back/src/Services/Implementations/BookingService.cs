using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using src.Data;
using src.DTOs.Bookings;
using src.DTOs.Common;
using src.Helpers;
using src.Models;
using src.Services.Interfaces;

namespace src.Services.Implementations;

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;
    private readonly UserManager<IdentityUser<int>> _userManager;
    private readonly IConfiguration _configuration;

    public BookingService(AppDbContext context, UserManager<IdentityUser<int>> userManager, IConfiguration configuration)
    {
        _context = context;
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<BookingResponse?> GetByIdAsync(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.WorkerProfile)
            .Include(b => b.WorkerService)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return null;

        var customer = await _userManager.FindByIdAsync(booking.CustomerId.ToString());
        var worker = await _userManager.FindByIdAsync(booking.WorkerProfile.UserId.ToString());

        return booking.ToResponse(
            customerName: customer?.UserName ?? "",
            workerName: worker?.UserName ?? "",
            serviceName: booking.WorkerService?.Title
        );
    }

    public async Task<PagedResponse<BookingResponse>> GetCustomerBookingsAsync(int customerId, BookingFilterRequest filter)
    {
        var query = _context.Bookings
            .Include(b => b.WorkerProfile)
            .Include(b => b.WorkerService)
            .Where(b => b.CustomerId == customerId);

        query = ApplyFilter(query, filter);

        return await PaginateBookingsAsync(query, filter.Page, filter.PageSize);
    }

    public async Task<PagedResponse<BookingResponse>> GetWorkerBookingsAsync(int workerProfileId, BookingFilterRequest filter)
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
        var commissionAmount = price * (decimal)(commissionPercentage / 100);

        var booking = new Booking
        {
            CustomerId = customerId,
            WorkerProfileId = request.WorkerProfileId,
            WorkerServiceId = request.WorkerServiceId,
            Status = "Pending",
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
        ValidateStatusTransition(booking.Status, "Accepted");

        booking.Status = "Accepted";
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(bookingId))!;
    }

    public async Task<BookingResponse> RejectAsync(int workerUserId, int bookingId, string? reason = null)
    {
        var booking = await GetWorkerBookingAsync(workerUserId, bookingId);
        ValidateStatusTransition(booking.Status, "Rejected");

        booking.Status = "Rejected";
        booking.CancelledAt = DateTime.UtcNow;
        booking.CancellationReason = reason;
        booking.UpdatedAt = DateTime.UtcNow;
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

        var cancellable = new[] { "Pending", "Accepted", "Scheduled" };
        if (!cancellable.Contains(booking.Status))
            throw new InvalidOperationException($"Cannot cancel booking in status '{booking.Status}'");

        booking.Status = "Cancelled";
        booking.CancelledAt = DateTime.UtcNow;
        booking.CancellationReason = reason;
        booking.CancelledBy = userId.ToString();
        booking.UpdatedAt = DateTime.UtcNow;
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

        var reschedulable = new[] { "Pending", "Accepted", "Scheduled" };
        if (!reschedulable.Contains(booking.Status))
            throw new InvalidOperationException($"Cannot reschedule booking in status '{booking.Status}'");

        booking.ScheduledAt = newScheduledAt;
        booking.Status = "Scheduled";
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(bookingId))!;
    }

    public async Task<BookingResponse> MarkOnTheWayAsync(int workerUserId, int bookingId)
    {
        var booking = await GetWorkerBookingAsync(workerUserId, bookingId);
        ValidateStatusTransition(booking.Status, "OnTheWay");

        booking.Status = "OnTheWay";
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(bookingId))!;
    }

    public async Task<BookingResponse> StartJobAsync(int workerUserId, int bookingId)
    {
        var booking = await GetWorkerBookingAsync(workerUserId, bookingId);
        ValidateStatusTransition(booking.Status, "Started");

        booking.Status = "Started";
        booking.StartedAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(bookingId))!;
    }

    public async Task<BookingResponse> PauseJobAsync(int workerUserId, int bookingId)
    {
        var booking = await GetWorkerBookingAsync(workerUserId, bookingId);
        ValidateStatusTransition(booking.Status, "Paused");

        booking.Status = "Paused";
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(bookingId))!;
    }

    public async Task<BookingResponse> CompleteJobAsync(int workerUserId, int bookingId)
    {
        var booking = await GetWorkerBookingAsync(workerUserId, bookingId);
        ValidateStatusTransition(booking.Status, "Completed");

        booking.Status = "Completed";
        booking.CompletedAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;

        var worker = await _context.WorkerProfiles.FindAsync(booking.WorkerProfileId);
        if (worker != null)
        {
            worker.CompletedJobs++;
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
            .Where(b => b.Status == "Pending" && b.CreatedAt <= cutoff)
            .ToListAsync();

        foreach (var booking in expired)
        {
            booking.Status = "Expired";
            booking.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    private async Task<Booking> GetWorkerBookingAsync(int workerUserId, int bookingId)
    {
        var profile = await _context.WorkerProfiles
            .FirstOrDefaultAsync(w => w.UserId == workerUserId)
            ?? throw new InvalidOperationException("Worker profile not found");

        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.WorkerProfileId == profile.Id)
            ?? throw new KeyNotFoundException("Booking not found");

        return booking;
    }

    private static void ValidateStatusTransition(string current, string next)
    {
        var allowed = new Dictionary<string, string[]>
        {
            ["Pending"] = ["Accepted", "Rejected", "Cancelled", "Expired"],
            ["Accepted"] = ["Scheduled", "Cancelled", "Rejected"],
            ["Scheduled"] = ["OnTheWay", "Cancelled"],
            ["OnTheWay"] = ["Started", "Cancelled"],
            ["Started"] = ["Paused", "Completed", "Cancelled"],
            ["Paused"] = ["Started", "Cancelled"]
        };

        if (allowed.TryGetValue(current, out var validNext) && validNext.Contains(next))
            return;

        throw new InvalidOperationException($"Cannot transition from '{current}' to '{next}'");
    }

    private IQueryable<Booking> ApplyFilter(IQueryable<Booking> query, BookingFilterRequest filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(b => b.Status == filter.Status);
        if (filter.FromDate.HasValue)
            query = query.Where(b => b.ScheduledAt >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            query = query.Where(b => b.ScheduledAt <= filter.ToDate.Value);

        return query.OrderByDescending(b => b.CreatedAt);
    }

    private async Task<PagedResponse<BookingResponse>> PaginateBookingsAsync(IQueryable<Booking> query, int page, int pageSize)
    {
        var totalCount = await query.CountAsync();
        var bookings = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = new List<BookingResponse>();
        foreach (var b in bookings)
        {
            var customer = await _userManager.FindByIdAsync(b.CustomerId.ToString());
            var worker = await _userManager.FindByIdAsync(b.WorkerProfile.UserId.ToString());
            items.Add(b.ToResponse(
                customerName: customer?.UserName ?? "",
                workerName: worker?.UserName ?? "",
                serviceName: b.WorkerService?.Title
            ));
        }

        return new PagedResponse<BookingResponse>
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