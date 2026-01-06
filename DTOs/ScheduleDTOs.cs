namespace Taxi_API.DTOs
{
    public record ScheduleEntry(string Name, string Address, double Lat, double Lng, System.DayOfWeek Day, string Time);
    public record CreateScheduleRequest(string? Name, ScheduleEntry[] Entries);
    public record ScheduledPlanResponse(Guid Id, string? Name, ScheduleEntry[] Entries);
}