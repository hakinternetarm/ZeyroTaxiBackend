using System.ComponentModel.DataAnnotations;

namespace Taxi_API.Models
{
    public class ScheduledPlan
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        public User? User { get; set; }

        // Optional name for the plan (e.g., "Work Week Commute")
        public string? Name { get; set; }

        // JSON serialized array of entries
        public string EntriesJson { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}