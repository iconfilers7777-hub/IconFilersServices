using IconFilers.Application;
using IconFilers.Application.DTOs;

namespace IconFilers.Api.IServices
{
    public interface IPaymentService
    {
        Task<CreatePaymentResponseDto> CreatePaymentAsync(CreatePaymentRequestDto request, string? idempotencyKey = null);
        Task<CapturePaymentResponseDto> CapturePaymentAsync(CapturePaymentRequestDto request);
    }
}
