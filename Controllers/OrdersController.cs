using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Taxi_API.Data;
using Taxi_API.Models;

namespace Taxi_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _db;

        public OrdersController(AppDbContext db)
        {
            _db = db;
        }

        private static double ToRadians(double deg) => deg * Math.PI / 180.0;

        private static double HaversineDistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371.0; // Earth radius in km
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private decimal CalculatePrice(double distanceKm, int etaMinutes, double pickupLat, double pickupLng, double destLat, double destLng)
        {
            // Simple pricing model: base fare + per km + per minute
            decimal baseFare = 400m; // base in local currency
            decimal perKm = 60m; // per km rate
            decimal perMinute = 20m; // per minute rate

            var price = baseFare + (decimal)distanceKm * perKm + (decimal)etaMinutes * perMinute;

            // Example zone surcharge: if pickup or destination within city center box apply 10% surcharge
            // (Replace boxes with real coordinates as needed)
            // City center box sample (lat/lng roughly)
            var cityCenterMinLat = 40.15; var cityCenterMaxLat = 40.25;
            var cityCenterMinLng = 44.45; var cityCenterMaxLng = 44.60;

            bool inCityZone = (pickupLat >= cityCenterMinLat && pickupLat <= cityCenterMaxLat && pickupLng >= cityCenterMinLng && pickupLng <= cityCenterMaxLng)
                              || (destLat >= cityCenterMinLat && destLat <= cityCenterMaxLat && destLng >= cityCenterMinLng && destLng <= cityCenterMaxLng);

            if (inCityZone)
            {
                price *= 1.10m; // 10% surcharge
            }

            // Minimum fare enforcement
            if (price < 800m) price = 800m;
            return Math.Round(price, 0);
        }

        [Authorize]
        [HttpPost("estimate")]
        public IActionResult Estimate([FromBody] Order order)
        {
            // Require coordinates for better estimate
            if (!order.PickupLat.HasValue || !order.DestLat.HasValue || !order.PickupLng.HasValue || !order.DestLng.HasValue)
            {
                return BadRequest("Coordinates required for estimate (PickupLat, PickupLng, DestLat, DestLng).");
            }

            var distance = HaversineDistanceKm(order.PickupLat.Value, order.PickupLng.Value, order.DestLat.Value, order.DestLng.Value);

            // ETA estimate based on average speed 30 km/h -> 0.5 km/min
            var eta = (int)Math.Ceiling(distance / 0.5);

            var price = CalculatePrice(distance, eta, order.PickupLat.Value, order.PickupLng.Value, order.DestLat.Value, order.DestLng.Value);

            return Ok(new { distanceKm = Math.Round(distance, 2), price = price, etaMinutes = eta });
        }

        [Authorize]
        [HttpPost("request")]
        public async Task<IActionResult> RequestOrder([FromBody] Order order)
        {
            // create order and start searching for driver
            var userIdStr = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            // Validate coordinates
            if (!order.PickupLat.HasValue || !order.DestLat.HasValue || !order.PickupLng.HasValue || !order.DestLng.HasValue)
            {
                return BadRequest("Coordinates required for ordering (PickupLat, PickupLng, DestLat, DestLng).");
            }

            order.Id = Guid.NewGuid();
            order.UserId = userId;
            order.CreatedAt = DateTime.UtcNow;
            order.Status = "searching";

            // Compute distance, eta and price
            var distance = HaversineDistanceKm(order.PickupLat.Value, order.PickupLng.Value, order.DestLat.Value, order.DestLng.Value);
            var eta = (int)Math.Ceiling(distance / 0.5);
            var price = CalculatePrice(distance, eta, order.PickupLat.Value, order.PickupLng.Value, order.DestLat.Value, order.DestLng.Value);

            order.DistanceKm = Math.Round(distance, 2);
            order.EtaMinutes = eta;
            order.Price = price;

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // Simulate driver search: pick first available driver (IsDriver==true)
            var driver = await _db.Users.FirstOrDefaultAsync(u => u.IsDriver && u.DriverProfile != null);
            if (driver != null)
            {
                order.DriverId = driver.Id;
                order.DriverName = driver.Name;
                order.DriverPhone = driver.Phone;
                order.DriverCar = "Toyota";
                order.DriverPlate = "510ZR10";
                order.Status = "assigned";
                order.EtaMinutes = 5;
                // Price remains calculated above
                await _db.SaveChangesAsync();

                // In real app, notify driver via push/SMS
            }

            return Ok(order);
        }

        [Authorize]
        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] string? reason)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            order.Status = "cancelled";
            order.CancelledAt = DateTime.UtcNow;
            order.CancelReason = reason;
            await _db.SaveChangesAsync();
            return Ok(order);
        }

        [Authorize]
        [HttpPost("driver/accept/{id}")]
        public async Task<IActionResult> DriverAccept(Guid id)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            order.Status = "on_trip";
            await _db.SaveChangesAsync();
            return Ok(order);
        }

        [Authorize]
        [HttpPost("complete/{id}")]
        public async Task<IActionResult> Complete(Guid id)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            order.Status = "completed";
            order.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(order);
        }

        [Authorize]
        [HttpPost("rate/{id}")]
        public async Task<IActionResult> Rate(Guid id, [FromBody] Order rating)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            order.Rating = rating.Rating;
            order.Review = rating.Review;
            await _db.SaveChangesAsync();
            return Ok(order);
        }
    }
}
