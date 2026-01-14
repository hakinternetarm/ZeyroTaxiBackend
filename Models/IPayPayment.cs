using System.ComponentModel.DataAnnotations;

namespace Taxi_API.Models
{
    public class IPayPayment
    {
        [Key]
        public int Id { get; set; }

        public Guid UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string OrderNumber { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string IPayOrderId { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        public decimal Amount { get; set; }

        [MaxLength(10)]
        public string Currency { get; set; } = "051"; // AMD

        [MaxLength(50)]
        public string Status { get; set; } = "Created"; // Created, Approved, Deposited, Declined, Reversed, Refunded

        [MaxLength(20)]
        public string? Pan { get; set; }

        [MaxLength(100)]
        public string? CardholderName { get; set; }

        [MaxLength(20)]
        public string? ApprovalCode { get; set; }

        public int? ActionCode { get; set; }

        [MaxLength(512)]
        public string? ActionCodeDescription { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public User? User { get; set; }
    }
}
