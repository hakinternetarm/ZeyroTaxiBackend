using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Taxi_API.Data;
using Taxi_API.Models;

namespace Taxi_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IdramController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<IdramController> _logger;

        public IdramController(AppDbContext db, IConfiguration config, ILogger<IdramController> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
        }

        // Generate HTML form data for redirect to Idram payment page
        [HttpPost("pay/{orderId}")]
        public async Task<IActionResult> Pay(Guid orderId)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return NotFound();

            // Read merchant config
            var edpRec = _config["Idram:RecAccount"];
            var successUrl = _config["Idram:SuccessUrl"];
            var failUrl = _config["Idram:FailUrl"];
            var resultUrl = _config["Idram:ResultUrl"];

            if (string.IsNullOrWhiteSpace(edpRec) || string.IsNullOrWhiteSpace(resultUrl))
                return StatusCode(500, "Idram not configured");

            // Prepare form values
            var amount = (order.Price ?? 0m).ToString(System.Globalization.CultureInfo.InvariantCulture);
            var billNo = order.BillNo ?? order.Id.ToString();

            var sb = new StringBuilder();
            sb.AppendLine("<!doctype html><html><body onload=\"document.forms[0].submit()\">" );
            sb.AppendLine($"<form action=\"https://banking.idram.am/Payment/GetPayment\" method=\"POST\">\n");
            sb.AppendLine($"<input type=\"hidden\" name=\"EDP_LANGUAGE\" value=\"EN\">\n");
            sb.AppendLine($"<input type=\"hidden\" name=\"EDP_REC_ACCOUNT\" value=\"{edpRec}\">\n");
            sb.AppendLine($"<input type=\"hidden\" name=\"EDP_DESCRIPTION\" value=\"Order {order.Id}\">\n");
            sb.AppendLine($"<input type=\"hidden\" name=\"EDP_AMOUNT\" value=\"{amount}\">\n");
            sb.AppendLine($"<input type=\"hidden\" name=\"EDP_BILL_NO\" value=\"{billNo}\">\n");
            if (!string.IsNullOrWhiteSpace(successUrl)) sb.AppendLine($"<input type=\"hidden\" name=\"EDP_SUCCESS_URL\" value=\"{successUrl}\">\n");
            if (!string.IsNullOrWhiteSpace(failUrl)) sb.AppendLine($"<input type=\"hidden\" name=\"EDP_FAIL_URL\" value=\"{failUrl}\">\n");
            sb.AppendLine("<input type=\"submit\" value=\"Pay\">\n</form></body></html>");

            return Content(sb.ToString(), "text/html");
        }

        // Endpoint for Idram result notifications (both precheck and final).
        [HttpPost("result")] // configured as RESULT_URL
        public async Task<IActionResult> Result()
        {
            // read form values
            var form = await Request.ReadFormAsync();
            var isPrecheck = form["EDP_PRECHECK"].FirstOrDefault() == "YES";
            var billNo = form["EDP_BILL_NO"].FirstOrDefault();
            var rec = form["EDP_REC_ACCOUNT"].FirstOrDefault();
            var amount = form["EDP_AMOUNT"].FirstOrDefault();

            if (isPrecheck)
            {
                // validate that bill exists and amount matches
                var order = await _db.Orders.FirstOrDefaultAsync(o => (o.BillNo ?? o.Id.ToString()) == billNo);
                if (order == null)
                {
                    _logger.LogWarning("Idram precheck: order not found {bill}", billNo);
                    return Content("ERROR");
                }

                var expected = (order.Price ?? 0m).ToString(System.Globalization.CultureInfo.InvariantCulture);
                if (expected != amount)
                {
                    _logger.LogWarning("Idram precheck: amount mismatch {bill} expected {exp} got {amt}", billNo, expected, amount);
                    return Content("ERROR");
                }

                return Content("OK"); // response must be plain OK
            }

            // final payment confirmation
            var payer = form["EDP_PAYER_ACCOUNT"].FirstOrDefault();
            var transId = form["EDP_TRANS_ID"].FirstOrDefault();
            var transDate = form["EDP_TRANS_DATE"].FirstOrDefault();
            var checksum = form["EDP_CHECKSUM"].FirstOrDefault();

            // validate checksum
            var secret = _config["Idram:SecretKey"] ?? string.Empty;
            var payload = string.Join(":", new[] { rec ?? string.Empty, amount ?? string.Empty, secret, billNo ?? string.Empty, payer ?? string.Empty, transId ?? string.Empty, transDate ?? string.Empty });
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var calc = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();

            if (calc != (checksum ?? string.Empty).ToLowerInvariant())
            {
                _logger.LogWarning("Idram checksum mismatch for bill {bill}", billNo);
                return Content("ERROR");
            }

            var order2 = await _db.Orders.FirstOrDefaultAsync(o => (o.BillNo ?? o.Id.ToString()) == billNo);
            if (order2 == null)
            {
                _logger.LogWarning("Idram confirm: order not found {bill}", billNo);
                return Content("ERROR");
            }

            // record payment
            order2.PaymentProvider = "idram";
            order2.TransactionId = transId;
            order2.PaidAt = DateTime.UtcNow;
            if (decimal.TryParse(amount, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var paid))
            {
                order2.PaidAmount = paid;
            }

            await _db.SaveChangesAsync();

            return Content("OK");
        }
    }
}
