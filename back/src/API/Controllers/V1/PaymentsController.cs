using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Payments;

namespace API.Controllers.V1;

[Authorize]
public class PaymentsController : BaseApiController
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("booking/{bookingId}")]
    public async Task<IActionResult> GetByBookingId(int bookingId)
    {
        var payment = await _paymentService.GetByBookingIdAsync(bookingId);
        if (payment == null) return NotFoundResult("Payment not found");
        return OkResult(payment);
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
    {
        var payment = await _paymentService.ProcessPaymentAsync(request.BookingId, request.PaymentMethod);
        return OkResult(payment);
    }

    [HttpPost("{bookingId}/refund")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Refund(int bookingId)
    {
        var result = await _paymentService.ProcessRefundAsync(bookingId);
        if (!result) return BadRequestResult("Refund failed or already processed");
        return OkResult(new { message = "Refund processed" });
    }
}
