namespace Taxi_API.Services
{
    public interface IPaymentService
    {
        // Tokenize card details and return a token string (should be sent to client to store securely)
        Task<string> TokenizeCardAsync(string cardNumber, int expMonth, int expYear, string cvc);

        // Charge a token for a given amount (in smallest currency unit), returns charge id
        Task<string> ChargeAsync(string token, long amountCents, string currency = "USD");

        // Refund a charge
        Task RefundAsync(string chargeId);

        // Create a PaymentIntent (for Stripe Elements / Payment Intents flow). Returns client_secret.
        Task<string?> CreatePaymentIntentAsync(long amountCents, string currency = "usd", string? customerId = null);

        // Transfer (payout) funds to a driver's connected account (instant/fast payout). Returns transfer id
        Task<string?> TransferToDriverAsync(long amountCents, string driverAccountId, string currency = "usd");

        // Create a connected account for a driver and return the account id
        Task<string?> CreateConnectedAccountAsync(string email, string country = "US");

        // Create an account link (onboarding) URL for a connected account
        Task<string?> CreateAccountLinkAsync(string accountId, string refreshUrl, string returnUrl);
    }
}
