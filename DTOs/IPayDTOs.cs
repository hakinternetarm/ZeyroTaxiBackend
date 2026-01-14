namespace Taxi_API.DTOs
{
    public class IPayPaymentRequest
    {
        public string OrderNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string Language { get; set; } = "en"; // en, ru, hy
    }

    public class IPayPaymentResponse
    {
        public string OrderId { get; set; } = string.Empty;
        public string FormUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
    }

    public class IPayOrderStatusRequest
    {
        public string OrderId { get; set; } = string.Empty;
    }

    public class IPayRefundRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
