namespace Taxi_API.DTOs
{
    public class IdramPaymentRequest
    {
        public string Language { get; set; } = "EN"; // RU, EN, AM
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string BillNo { get; set; } = string.Empty;
        public string? Email { get; set; }
    }

    public class IdramPaymentFormData
    {
        public string PaymentUrl { get; set; } = string.Empty;
        public Dictionary<string, string> FormFields { get; set; } = new();
    }

    public class IdramPrecheckRequest
    {
        public string EDP_PRECHECK { get; set; } = string.Empty;
        public string EDP_BILL_NO { get; set; } = string.Empty;
        public string EDP_REC_ACCOUNT { get; set; } = string.Empty;
        public string EDP_AMOUNT { get; set; } = string.Empty;
    }

    public class IdramPaymentConfirmation
    {
        public string EDP_BILL_NO { get; set; } = string.Empty;
        public string EDP_REC_ACCOUNT { get; set; } = string.Empty;
        public string EDP_PAYER_ACCOUNT { get; set; } = string.Empty;
        public string EDP_AMOUNT { get; set; } = string.Empty;
        public string EDP_TRANS_ID { get; set; } = string.Empty;
        public string EDP_TRANS_DATE { get; set; } = string.Empty;
        public string EDP_CHECKSUM { get; set; } = string.Empty;
    }
}
