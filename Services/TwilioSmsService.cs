using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Taxi_API.Services
{
    public class TwilioSmsService : ISmsService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<TwilioSmsService> _logger;

        public TwilioSmsService(IConfiguration config, ILogger<TwilioSmsService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendSmsAsync(string toPhone, string body)
        {
            var accountSid = _config["Twilio:AccountSid"];
            var authToken = _config["Twilio:AuthToken"];
            var from = _config["Twilio:From"];

            if (string.IsNullOrWhiteSpace(accountSid) || string.IsNullOrWhiteSpace(authToken) || string.IsNullOrWhiteSpace(from))
            {
                _logger.LogInformation("Twilio not configured, skipping SMS to {to}", toPhone);
                return;
            }

            try
            {
                // Lazy-reference Twilio to avoid adding package unless configured
                var clientType = Type.GetType("Twilio.Twilio, Twilio");
                if (clientType == null)
                {
                    _logger.LogWarning("Twilio SDK not available. Install Twilio package to enable SMS.");
                    return;
                }

                // Use reflection to avoid hard dependency in this sample
                dynamic client = Activator.CreateInstance(clientType, accountSid, authToken);
                // Twilio SDK usage would be: Twilio.Rest.Api.V2010.Account.MessageResource.Create(...)
                // In production add the Twilio NuGet package and implement directly.
                _logger.LogInformation("Pretend to send SMS via Twilio to {to}: {body}", toPhone, body);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send SMS to {to}", toPhone);
            }

            await Task.CompletedTask;
        }
    }
}
