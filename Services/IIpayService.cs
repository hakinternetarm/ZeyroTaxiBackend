using System.Text.Json;

namespace Taxi_API.Services
{
    public interface IIpayService
    {
        // Register an order (register.do)
        Task<JsonDocument?> RegisterOrderAsync(string orderNumber, long amountCents, string returnUrl, string? description = null, string? language = null, string? pageView = null, string? clientId = null, string? jsonParams = null, int? sessionTimeoutSecs = null);

        // Register order with pre-auth (registerPreAuth.do)
        Task<JsonDocument?> RegisterPreAuthAsync(string orderNumber, long amountCents, string returnUrl, string? description = null, string? language = null, string? pageView = null, string? clientId = null, string? jsonParams = null, int? sessionTimeoutSecs = null);

        // Payment using card details (paymentorder.do) - returns JSON which may include acsUrl/cReq/redirect
        Task<JsonDocument?> PaymentOrderAsync(string mdOrder, string pan, string cvc, string yyyy, string mm, string text, string? language = null, string? ip = null, string? jsonParams = null);

        // Get order status extended
        Task<JsonDocument?> GetOrderStatusExtendedAsync(string? orderId = null, string? orderNumber = null, string? language = null);

        // Deposit (complete preauth)
        Task<JsonDocument?> DepositAsync(string orderId, long? amountCents = null);

        // Reverse
        Task<JsonDocument?> ReverseAsync(string orderId);

        // Refund
        Task<JsonDocument?> RefundAsync(string orderId, long amountCents);
    }
}
