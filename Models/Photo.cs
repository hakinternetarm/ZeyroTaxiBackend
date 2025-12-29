using System.ComponentModel.DataAnnotations;

namespace Taxi_API.Models
{
    public class Photo
    {
        public int Id { get; set; }

        public Guid UserId { get; set; }

        // optional link to the driver profile this photo belongs to
        public int? DriverProfileId { get; set; }
        public DriverProfile? DriverProfile { get; set; }

        public string Path { get; set; } = null!;

        public string Type { get; set; } = null!; // passport_front, passport_back, dl_front, dl_back, car_front, car_back, car_left, car_right, car_interior, tech_passport

        public long Size { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}