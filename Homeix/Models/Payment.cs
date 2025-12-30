using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homeix.Models
{
    [Table("Payment")]
    public class Payment
    {
        [Key]
        [Column("PaymentID")]
        public int PaymentId { get; set; }

        [Required]
        [Column("UserID")]
        public int UserId { get; set; }

        [Required]
        [Column("SubscriptionID")]
        public int SubscriptionId { get; set; }

        [Required]
        [Column("PaymentMethodID")]
        public int PaymentMethodId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        [Column(TypeName = "datetime")]
        public DateTime PaymentDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Completed";

        // Navigation
        public virtual PaymentMethod? PaymentMethod { get; set; }
        public virtual Subscription? Subscription { get; set; }
        public virtual User? User { get; set; }
    }
}
