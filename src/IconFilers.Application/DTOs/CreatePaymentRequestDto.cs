namespace IconFilers.Application.DTOs;

public class CreatePaymentRequestDto
{
    public int ClientId { get; set; }
    public decimal Amount { get; set; }      // base amount before tax/discount
    public decimal TaxAmount { get; set; }
    public decimal Discount { get; set; }
    public string Currency { get; set; } = "USD";
    public string ReturnUrl { get; set; } = "";
    public string CancelUrl { get; set; } = "";

}