using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Taxi_API.Services
{
    // Minimal IPay REST client implementation. Replace with production-hardened client as needed.
    public class IpayService : IIpayService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ILogger<IpayService> _logger;
        private readonly string _baseUrl;
        private readonly string _userName;
        private readonly string _password;

        public IpayService(IConfiguration config, IHttpClientFactory httpFactory, ILogger<IpayService> logger)
        {
            _config = config;
            _logger = logger;
            _baseUrl = config["Ipay:BaseUrl"] ?? "https://ipay.arca.am/payment/rest";
            _userName = config["Ipay:UserName"] ?? string.Empty;
            _password = config["Ipay:Password"] ?? string.Empty;
            _http = httpFactory.CreateClient();
        }

        private async Task<JsonDocument?> PostFormAsync(string path, IEnumerable<KeyValuePair<string, string>> data)
        {
            try
            {
                var url = _baseUrl.TrimEnd('/') + "/" + path.TrimStart('/');
                var content = new FormUrlEncodedContent(data);
                var res = await _http.PostAsync(url, content);
                var txt = await res.Content.ReadAsStringAsync();
                _logger.LogDebug("Ipay POST {url} => {status} {body}", url, res.StatusCode, txt);
                if (!res.IsSuccessStatusCode) return null;
                return JsonDocument.Parse(txt);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ipay PostForm failed");
                return null;
            }
        }

        public Task<JsonDocument?> RegisterOrderAsync(string orderNumber, long amountCents, string returnUrl, string? description = null, string? language = null, string? pageView = null, string? clientId = null, string? jsonParams = null, int? sessionTimeoutSecs = null)
        {
            var data = new List<KeyValuePair<string, string>>
            {
                new("userName", _userName),
                new("password", _password),
                new("orderNumber", orderNumber),
                new("amount", amountCents.ToString()),
                new("returnUrl", returnUrl)
            };
            if (!string.IsNullOrWhiteSpace(description)) data.Add(new KeyValuePair<string,string>("description", description));
            if (!string.IsNullOrWhiteSpace(language)) data.Add(new KeyValuePair<string,string>("language", language));
            if (!string.IsNullOrWhiteSpace(pageView)) data.Add(new KeyValuePair<string,string>("pageView", pageView));
            if (!string.IsNullOrWhiteSpace(clientId)) data.Add(new KeyValuePair<string,string>("clientId", clientId));
            if (!string.IsNullOrWhiteSpace(jsonParams)) data.Add(new KeyValuePair<string,string>("jsonParams", jsonParams));
            if (sessionTimeoutSecs.HasValue) data.Add(new KeyValuePair<string,string>("sessionTimeoutSecs", sessionTimeoutSecs.Value.ToString()));
            return PostFormAsync("register.do", data);
        }

        public Task<JsonDocument?> RegisterPreAuthAsync(string orderNumber, long amountCents, string returnUrl, string? description = null, string? language = null, string? pageView = null, string? clientId = null, string? jsonParams = null, int? sessionTimeoutSecs = null)
        {
            var data = new List<KeyValuePair<string, string>>
            {
                new("userName", _userName),
                new("password", _password),
                new("orderNumber", orderNumber),
                new("amount", amountCents.ToString()),
                new("returnUrl", returnUrl)
            };
            if (!string.IsNullOrWhiteSpace(description)) data.Add(new KeyValuePair<string,string>("description", description));
            if (!string.IsNullOrWhiteSpace(language)) data.Add(new KeyValuePair<string,string>("language", language));
            if (!string.IsNullOrWhiteSpace(pageView)) data.Add(new KeyValuePair<string,string>("pageView", pageView));
            if (!string.IsNullOrWhiteSpace(clientId)) data.Add(new KeyValuePair<string,string>("clientId", clientId));
            if (!string.IsNullOrWhiteSpace(jsonParams)) data.Add(new KeyValuePair<string,string>("jsonParams", jsonParams));
            if (sessionTimeoutSecs.HasValue) data.Add(new KeyValuePair<string,string>("sessionTimeoutSecs", sessionTimeoutSecs.Value.ToString()));
            return PostFormAsync("registerPreAuth.do", data);
        }

        public Task<JsonDocument?> PaymentOrderAsync(string mdOrder, string pan, string cvc, string yyyy, string mm, string text, string? language = null, string? ip = null, string? jsonParams = null)
        {
            var data = new List<KeyValuePair<string, string>>
            {
                new("userName", _userName),
                new("password", _password),
                new("MDORDER", mdOrder),
                new("$PAN", pan),
                new("$CVC", cvc),
                new("YYYY", yyyy),
                new("MM", mm),
                new("TEXT", text),
            };
            if (!string.IsNullOrWhiteSpace(language)) data.Add(new KeyValuePair<string,string>("language", language));
            if (!string.IsNullOrWhiteSpace(ip)) data.Add(new KeyValuePair<string,string>("ip", ip));
            if (!string.IsNullOrWhiteSpace(jsonParams)) data.Add(new KeyValuePair<string,string>("jsonParams", jsonParams));
            return PostFormAsync("paymentorder.do", data);
        }

        public Task<JsonDocument?> GetOrderStatusExtendedAsync(string? orderId = null, string? orderNumber = null, string? language = null)
        {
            var data = new List<KeyValuePair<string, string>>
            {
                new("userName", _userName),
                new("password", _password)
            };
            if (!string.IsNullOrWhiteSpace(orderId)) data.Add(new KeyValuePair<string,string>("orderId", orderId));
            if (!string.IsNullOrWhiteSpace(orderNumber)) data.Add(new KeyValuePair<string,string>("orderNumber", orderNumber));
            if (!string.IsNullOrWhiteSpace(language)) data.Add(new KeyValuePair<string,string>("language", language));
            return PostFormAsync("getOrderStatusExtended.do", data);
        }

        public Task<JsonDocument?> DepositAsync(string orderId, long? amountCents = null)
        {
            var data = new List<KeyValuePair<string, string>>
            {
                new("userName", _userName),
                new("password", _password),
                new("orderId", orderId)
            };
            if (amountCents.HasValue) data.Add(new KeyValuePair<string,string>("amount", amountCents.Value.ToString()));
            return PostFormAsync("deposit.do", data);
        }

        public Task<JsonDocument?> ReverseAsync(string orderId)
        {
            var data = new List<KeyValuePair<string, string>>
            {
                new("userName", _userName),
                new("password", _password),
                new("orderId", orderId)
            };
            return PostFormAsync("reverse.do", data);
        }

        public Task<JsonDocument?> RefundAsync(string orderId, long amountCents)
        {
            var data = new List<KeyValuePair<string, string>>
            {
                new("userName", _userName),
                new("password", _password),
                new("orderId", orderId),
                new("amount", amountCents.ToString())
            };
            return PostFormAsync("refund.do", data);
        }
    }
}
