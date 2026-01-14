using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Taxi_API.Services
{
    public class IdramPaymentService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<IdramPaymentService> _logger;

        public IdramPaymentService(IConfiguration config, ILogger<IdramPaymentService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public string GetReceiverAccount()
        {
            return _config["Idram:ReceiverAccount"] ?? "100000114";
        }

        public string GetPaymentUrl()
        {
            return _config["Idram:PaymentUrl"] ?? "https://banking.idram.am/Payment/GetPayment";
        }

        public string GetSuccessUrl()
        {
            return _config["Idram:SuccessUrl"] ?? "http://localhost:5000/api/idram/success";
        }

        public string GetFailUrl()
        {
            return _config["Idram:FailUrl"] ?? "http://localhost:5000/api/idram/fail";
        }

        public string GetResultUrl()
        {
            return _config["Idram:ResultUrl"] ?? "http://localhost:5000/api/idram/result";
        }

        public string GetEmail()
        {
            return _config["Idram:Email"] ?? string.Empty;
        }

        /// <summary>
        /// Validates the checksum for payment confirmation requests from Idram
        /// </summary>
        public bool ValidateChecksum(
            string recAccount,
            string amount,
            string billNo,
            string payerAccount,
            string transId,
            string transDate,
            string checksum)
        {
            var secretKey = _config["Idram:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                _logger.LogWarning("Idram SecretKey not configured");
                return false;
            }

            // EDP_CHECKSUM calculation as per Idram docs:
            // MD5(EDP_REC_ACCOUNT:EDP_AMOUNT:SECRET_KEY:EDP_BILL_NO:EDP_PAYER_ACCOUNT:EDP_TRANS_ID:EDP_TRANS_DATE)
            var concatenated = $"{recAccount}:{amount}:{secretKey}:{billNo}:{payerAccount}:{transId}:{transDate}";
            var calculatedChecksum = CalculateMD5(concatenated);

            var isValid = string.Equals(calculatedChecksum, checksum, StringComparison.OrdinalIgnoreCase);
            
            if (!isValid)
            {
                _logger.LogWarning("Idram checksum validation failed. Expected: {expected}, Got: {actual}", 
                    calculatedChecksum, checksum);
            }

            return isValid;
        }

        /// <summary>
        /// Validates order authenticity for preliminary check
        /// </summary>
        public bool ValidateOrderPrecheck(string billNo, string recAccount, string amount)
        {
            // Implement your business logic to validate if this order exists and is valid
            // For now, basic validation
            if (string.IsNullOrWhiteSpace(billNo) || string.IsNullOrWhiteSpace(amount))
            {
                return false;
            }

            var configuredRecAccount = GetReceiverAccount();
            if (!string.Equals(recAccount, configuredRecAccount, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Receiver account mismatch. Expected: {expected}, Got: {actual}",
                    configuredRecAccount, recAccount);
                return false;
            }

            // Add additional validation logic here (e.g., check if order exists in database)
            return true;
        }

        private string CalculateMD5(string input)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                return Convert.ToHexString(hashBytes).ToLowerInvariant();
            }
        }
    }
}
