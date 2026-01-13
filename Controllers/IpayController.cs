using Microsoft.AspNetCore.Mvc;
using Taxi_API.Services;
using Taxi_API.Data;
using Microsoft.EntityFrameworkCore;

namespace Taxi_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IpayController : ControllerBase
    {
        private readonly IIpayService _ipay;
        private readonly AppDbContext _db;

        public IpayController(IIpayService ipay, AppDbContext db)
        {
            _ipay = ipay;
            _db = db;
        }

        // Register an order and return redirect formUrl
        [HttpPost("register/{orderId}")]
        public async Task<IActionResult> RegisterOrder(Guid orderId, [FromBody] string? returnUrl)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return NotFound();
            var orderNumber = order.BillNo ?? order.Id.ToString();
            var amountCents = (long)((order.Price ?? 0m) * 100);
            var res = await _ipay.RegisterOrderAsync(orderNumber, amountCents, returnUrl ?? "");
            if (res == null) return StatusCode(502, "Ipay register failed");
            // return JSON
            return Content(res.RootElement.ToString(), "application/json");
        }

        [HttpPost("status/{orderId}")]
        public async Task<IActionResult> Status(Guid orderId)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return NotFound();
            var orderNumber = order.BillNo ?? order.Id.ToString();
            var res = await _ipay.GetOrderStatusExtendedAsync(orderNumber: orderNumber);
            if (res == null) return StatusCode(502, "Ipay status failed");
            return Content(res.RootElement.ToString(), "application/json");
        }

        // Endpoint for form-based payment (paymentorder) where merchant collects card details and posts to gateway
        [HttpPost("paymentorder/{orderId}")]
        public async Task<IActionResult> PaymentOrder(Guid orderId, [FromForm] string pan, [FromForm] string cvc, [FromForm] string yyyy, [FromForm] string mm, [FromForm] string text)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return NotFound();
            var mdOrder = order.Id.ToString();
            var res = await _ipay.PaymentOrderAsync(mdOrder, pan, cvc, yyyy, mm, text);
            if (res == null) return StatusCode(502, "Ipay payment failed");
            return Content(res.RootElement.ToString(), "application/json");
        }
    }
}
