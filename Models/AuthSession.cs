using System.ComponentModel.DataAnnotations;

namespace Taxi_API.Models
{
    public class AuthSession
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Phone { get; set; } = null!;

        // numeric code sent to user
        public string Code { get; set; } = null!;

        public bool Verified { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
    }
}