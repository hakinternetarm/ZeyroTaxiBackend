using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Taxi_API.Data;
using Taxi_API.DTOs;
using Taxi_API.Models;

namespace Taxi_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ScheduleController(AppDbContext db)
        {
            _db = db;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleRequest req)
        {
            var userIdStr = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            if (req == null || req.Entries == null || req.Entries.Length == 0) return BadRequest("Entries required");

            var plan = new ScheduledPlan
            {
                UserId = userId,
                Name = req.Name,
                EntriesJson = JsonSerializer.Serialize(req.Entries),
                CreatedAt = DateTime.UtcNow
            };
            _db.ScheduledPlans.Add(plan);
            await _db.SaveChangesAsync();

            var entries = JsonSerializer.Deserialize<ScheduleEntry[]>(plan.EntriesJson) ?? Array.Empty<ScheduleEntry>();
            return Ok(new ScheduledPlanResponse(plan.Id, plan.Name, entries));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetSchedules()
        {
            var userIdStr = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var plans = await _db.ScheduledPlans.Where(s => s.UserId == userId).ToListAsync();
            var res = plans.Select(p => {
                var entries = JsonSerializer.Deserialize<ScheduleEntry[]>(p.EntriesJson) ?? Array.Empty<ScheduleEntry>();
                return new ScheduledPlanResponse(p.Id, p.Name, entries);
            }).ToArray();

            return Ok(res);
        }
    }
}
