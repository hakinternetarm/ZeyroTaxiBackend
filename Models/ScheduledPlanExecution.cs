using System.ComponentModel.DataAnnotations;

namespace Taxi_API.Models
{
    public class ScheduledPlanExecution
    {
        [Key]
        public int Id { get; set; }

        public Guid PlanId { get; set; }
        public ScheduledPlan? Plan { get; set; }

        // index of the entry within the plan's entries array
        public int EntryIndex { get; set; }

        // the date (UTC date) of the occurrence that was executed
        public DateTime OccurrenceDate { get; set; }

        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    }
}