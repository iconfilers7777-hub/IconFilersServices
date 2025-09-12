// IconFilers.Application/DTOs/PaymentDtos.cs
namespace IconFilers.Application.DTOs
{
    public record PaymentDto(
        int Id,
        int ClientId,
        decimal Amount,
        decimal TaxAmount,
        decimal Discount,
        decimal NetAmount,
        string Currency,
        DateTime PaymentDate,
        int? PaymentModeId,
        string? PaymentModeName,
        string? Reference,
        string? Status
    );

    public record CreatePaymentDto(
        int ClientId,
        decimal Amount,
        DateTime PaymentDate,
        int? PaymentModeId = null,
        string? Reference = null,
        decimal TaxAmount = 0m,
        decimal Discount = 0m
    );

    public record UpdatePaymentDto(
        int Id,
        string? Status = null,
        decimal? Amount = null,
        int? PaymentModeId = null
    );
}
