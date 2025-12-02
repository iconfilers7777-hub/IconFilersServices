using IconFilers.Api.IServices;
using IconFilers.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace IconFilers.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin,Client")]
        public async Task<IActionResult> Create([FromBody] CreatePaymentRequestDto dto)
        {
            try
            {
                // Validate required string fields
                if (string.IsNullOrWhiteSpace(dto.Currency) || string.IsNullOrWhiteSpace(dto.ReturnUrl) || string.IsNullOrWhiteSpace(dto.CancelUrl))
                {
                    _logger.LogWarning("Create payment validation failed: missing required fields. ClientId:{ClientId}", dto.ClientId);
                    return BadRequest("Currency, ReturnUrl, and CancelUrl are required.");
                }
                // Accept optional Idempotency-Key header
                var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault();
                var result = await _paymentService.CreatePaymentAsync(dto, idempotencyKey);
                return Ok(result);
            }
            catch (ArgumentException aex)
            {
                _logger.LogWarning(aex, "Create payment argument error. ClientId:{ClientId}", dto.ClientId);
                return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create payment failed. ClientId:{ClientId}", dto.ClientId);
                return StatusCode(500, "Create payment failed");
            }
        }

        [HttpPost("capture")]
        [Authorize(Roles = "Admin,Client")]
        public async Task<IActionResult> Capture([FromBody] CapturePaymentRequestDto dto)
        {
            try
            {
                // Validate required string fields
                if (string.IsNullOrWhiteSpace(dto.OrderId))
                {
                    _logger.LogWarning("Capture payment validation failed: missing OrderId. PaymentId:{PaymentId}", dto.PaymentId);
                    return BadRequest("OrderId is required.");
                }
                var result = await _paymentService.CapturePaymentAsync(dto);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Capture payment not found. PaymentId:{PaymentId} OrderId:{OrderId}", dto.PaymentId, dto.OrderId);
                return NotFound("Payment not found");
            }
            catch (InvalidOperationException ioex)
            {
                _logger.LogWarning(ioex, "Capture payment invalid operation. PaymentId:{PaymentId} OrderId:{OrderId}", dto.PaymentId, dto.OrderId);
                return BadRequest(ioex.Message); // amount mismatch or business rule
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Capture failed. PaymentId:{PaymentId} OrderId:{OrderId}", dto.PaymentId, dto.OrderId);
                return StatusCode(500, "Capture failed");
            }
        }
    }
}
