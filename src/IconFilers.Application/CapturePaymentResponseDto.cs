namespace IconFilers.Application
{
    public class CapturePaymentResponseDto
    {
        public string Status { get; set; } = "";
        public decimal CapturedAmount { get; set; }
        public string Currency { get; set; } = "";
        public object? RawResponse { get; set; }
    }
}
