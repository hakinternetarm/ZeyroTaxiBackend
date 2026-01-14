using System.ComponentModel.DataAnnotations;

namespace Taxi_API.Models
{
    public class IdramPayment
    {
        [Key]
        public int Id { get; set; }

        public Guid UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string BillNo { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        public decimal Amount { get; set; }

        [MaxLength(20)]
        public string? PayerAccount { get; set; }

        [MaxLength(50)]
        public string? TransactionId { get; set; }

        [MaxLength(20)]
        public string? TransactionDate { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Success, Failed

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public User? User { get; set; }
    }
}
