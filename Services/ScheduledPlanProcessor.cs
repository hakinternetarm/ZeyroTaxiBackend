using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Taxi_API.Data;
using Taxi_API.DTOs;
using Taxi_API.Models;

namespace Taxi_API.Services
{
    // Background service that periodically checks scheduled plans and creates Orders when occurrences are due.
    public class ScheduledPlanProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ScheduledPlanProcessor> _logger;
        private readonly ISocketService _socketService;

        public ScheduledPlanProcessor(IServiceScopeFactory scopeFactory, ILogger<ScheduledPlanProcessor> logger, ISocketService socketService)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _socketService = socketService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ScheduledPlanProcessor started");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOnceAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ScheduledPlanProcessor");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            _logger.LogInformation("ScheduledPlanProcessor stopped");
        }

        private async Task ProcessOnceAsync(CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            // consider a small window, e.g., entries scheduled for the next 1 minute
            var windowStart = now;
            var windowEnd = now.AddMinutes(1);

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var plans = await db.ScheduledPlans.ToListAsync(ct);
            foreach (var plan in plans)
            {
                ScheduleEntry[] entries;
                try
                {
                    entries = JsonSerializer.Deserialize<ScheduleEntry[]>(plan.EntriesJson) ?? Array.Empty<ScheduleEntry>();
                }
                catch
                {
                    continue;
                }

                for (int i = 0; i < entries.Length; i++)
                {
                    var e = entries[i];
                    // build next occurrence for this entry in UTC based on DayOfWeek and Time string (HH:mm)
                    if (!TimeSpan.TryParse(e.Time, out var timeOfDay)) continue;

                    var next = NextOccurrenceUtc(e.Day, timeOfDay, now);
                    if (next < windowStart || next > windowEnd) continue;

                    // check if already executed for this occurrence
                    var already = await db.ScheduledPlanExecutions.AnyAsync(x => x.PlanId == plan.Id && x.EntryIndex == i && x.OccurrenceDate == next.Date, ct);
                    if (already) continue;

                    // create order for this occurrence
                    var order = new Order
                    {
                        Id = Guid.NewGuid(),
                        UserId = plan.UserId,
                        CreatedAt = DateTime.UtcNow,
                        Action = "taxi",
                        Pickup = e.Address,
                        Destination = e.Address,
                        PickupLat = e.Lat,
                        PickupLng = e.Lng,
                        DestLat = e.Lat,
                        DestLng = e.Lng,
                        Status = "searching"
                    };

                    // compute estimates
                    var distance = 0.0;
                    var eta = 1;
                    var price = 800m;
                    order.DistanceKm = distance;
                    order.EtaMinutes = eta;
                    order.Price = price;

                    db.Orders.Add(order);
                    await db.SaveChangesAsync(ct);

                    // mark executed
                    db.ScheduledPlanExecutions.Add(new ScheduledPlanExecution { PlanId = plan.Id, EntryIndex = i, OccurrenceDate = next.Date, ExecutedAt = DateTime.UtcNow });
                    await db.SaveChangesAsync(ct);

                    // notify via socket
                    await _socketService.NotifyOrderEventAsync(order.Id, "carFinding", new { status = "searching" });

                    // try assign driver
                    var driver = await db.Users.FirstOrDefaultAsync(u => u.IsDriver && u.DriverProfile != null, ct);
                    if (driver != null)
                    {
                        order.DriverId = driver.Id;
                        order.DriverName = driver.Name;
                        order.DriverPhone = driver.Phone;
                        order.DriverCar = "Toyota";
                        order.DriverPlate = "510ZR10";
                        order.Status = "assigned";
                        order.EtaMinutes = 5;
                        await db.SaveChangesAsync(ct);

                        await _socketService.NotifyOrderEventAsync(order.Id, "carFound", new { driver = new { id = driver.Id, name = driver.Name, phone = driver.Phone } });
                    }
                }
            }
        }

        private static DateTime NextOccurrenceUtc(DayOfWeek day, TimeSpan timeOfDay, DateTime now)
        {
            // find next date (including today) which has given DayOfWeek
            var daysDiff = ((int)day - (int)now.DayOfWeek + 7) % 7;
            var candidateDate = now.Date.AddDays(daysDiff).Add(timeOfDay);
            if (candidateDate < now) candidateDate = candidateDate.AddDays(7);
            return DateTime.SpecifyKind(candidateDate, DateTimeKind.Utc);
        }
    }
}
