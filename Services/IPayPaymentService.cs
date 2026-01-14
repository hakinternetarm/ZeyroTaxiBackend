using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Taxi_API.Services
{
    public class IPayPaymentService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<IPayPaymentService> _logger;
        private readonly HttpClient _httpClient;

        public IPayPaymentService(IConfiguration config, ILogger<IPayPaymentService> logger, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        public string GetUserName()
        {
            return _config["IPay:UserName"] ?? string.Empty;
        }

        public string GetPassword()
        {
            return _config["IPay:Password"] ?? string.Empty;
        }

        public string GetApiBaseUrl()
        {
            return _config["IPay:ApiBaseUrl"] ?? "https://ipay.arca.am/payment/rest";
        }

        public string GetReturnUrl()
        {
            return _config["IPay:ReturnUrl"] ?? "http://localhost:5000/api/ipay/return";
        }

        public string GetCurrency()
        {
            return _config["IPay:Currency"] ?? "051"; // AMD - Armenian Dram
        }

        public string GetLanguage()
        {
            return _config["IPay:Language"] ?? "en";
        }

        /// <summary>
        /// Register a new order with IPay
        /// </summary>
        public async Task<IPayRegisterResponse?> RegisterOrderAsync(string orderNumber, decimal amount, string? description = null)
        {
            var url = $"{GetApiBaseUrl()}/register.do";
            
            var parameters = new Dictionary<string, string>
            {
                { "userName", GetUserName() },
                { "password", GetPassword() },
                { "orderNumber", orderNumber },
                { "amount", ((long)(amount * 100)).ToString() }, // Convert to minor units (cents/lumis)
                { "currency", GetCurrency() },
                { "returnUrl", GetReturnUrl() },
                { "language", GetLanguage() }
            };

            if (!string.IsNullOrWhiteSpace(description))
            {
                parameters.Add("description", description);
            }

            try
            {
                var content = new FormUrlEncodedContent(parameters);
                var response = await _httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("IPay register response: {response}", responseBody);

                var result = JsonSerializer.Deserialize<IPayRegisterResponse>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering order with IPay");
                return null;
            }
        }

        /// <summary>
        /// Get order status from IPay
        /// </summary>
        public async Task<IPayOrderStatusResponse?> GetOrderStatusAsync(string orderId)
        {
            var url = $"{GetApiBaseUrl()}/getOrderStatusExtended.do";
            
            var parameters = new Dictionary<string, string>
            {
                { "userName", GetUserName() },
                { "password", GetPassword() },
                { "orderId", orderId },
                { "language", GetLanguage() }
            };

            try
            {
                var content = new FormUrlEncodedContent(parameters);
                var response = await _httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("IPay status response: {response}", responseBody);

                var result = JsonSerializer.Deserialize<IPayOrderStatusResponse>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order status from IPay");
                return null;
            }
        }

        /// <summary>
        /// Reverse (cancel) a payment
        /// </summary>
        public async Task<IPayOperationResponse?> ReversePaymentAsync(string orderId)
        {
            var url = $"{GetApiBaseUrl()}/reverse.do";
            
            var parameters = new Dictionary<string, string>
            {
                { "userName", GetUserName() },
                { "password", GetPassword() },
                { "orderId", orderId }
            };

            try
            {
                var content = new FormUrlEncodedContent(parameters);
                var response = await _httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("IPay reverse response: {response}", responseBody);

                var result = JsonSerializer.Deserialize<IPayOperationResponse>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reversing payment with IPay");
                return null;
            }
        }

        /// <summary>
        /// Refund a payment
        /// </summary>
        public async Task<IPayOperationResponse?> RefundPaymentAsync(string orderId, decimal amount)
        {
            var url = $"{GetApiBaseUrl()}/refund.do";
            
            var parameters = new Dictionary<string, string>
            {
                { "userName", GetUserName() },
                { "password", GetPassword() },
                { "orderId", orderId },
                { "amount", ((long)(amount * 100)).ToString() } // Convert to minor units
            };

            try
            {
                var content = new FormUrlEncodedContent(parameters);
                var response = await _httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("IPay refund response: {response}", responseBody);

                var result = JsonSerializer.Deserialize<IPayOperationResponse>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding payment with IPay");
                return null;
            }
        }

        /// <summary>
        /// Deposit (complete) a pre-authorized payment
        /// </summary>
        public async Task<IPayOperationResponse?> DepositPaymentAsync(string orderId, decimal? amount = null)
        {
            var url = $"{GetApiBaseUrl()}/deposit.do";
            
            var parameters = new Dictionary<string, string>
            {
                { "userName", GetUserName() },
                { "password", GetPassword() },
                { "orderId", orderId }
            };

            if (amount.HasValue)
            {
                parameters.Add("amount", ((long)(amount.Value * 100)).ToString());
            }

            try
            {
                var content = new FormUrlEncodedContent(parameters);
                var response = await _httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("IPay deposit response: {response}", responseBody);

                var result = JsonSerializer.Deserialize<IPayOperationResponse>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error depositing payment with IPay");
                return null;
            }
        }
    }

    // Response models
    public class IPayRegisterResponse
    {
        public string? OrderId { get; set; }
        public string? FormUrl { get; set; }
        public int ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public bool Error { get; set; }
    }

    public class IPayOrderStatusResponse
    {
        public string? OrderNumber { get; set; }
        public int OrderStatus { get; set; }
        public int ActionCode { get; set; }
        public string? ActionCodeDescription { get; set; }
        public int ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public long Amount { get; set; }
        public string? Currency { get; set; }
        public DateTime Date { get; set; }
        public string? OrderDescription { get; set; }
        public string? Ip { get; set; }
        public CardAuthInfo? CardAuthInfo { get; set; }
    }

    public class CardAuthInfo
    {
        public string? Pan { get; set; }
        public string? Expiration { get; set; }
        public string? CardholderName { get; set; }
        public string? ApprovalCode { get; set; }
    }

    public class IPayOperationResponse
    {
        public int ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public bool Error { get; set; }
    }
}
