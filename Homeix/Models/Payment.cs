using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Homeix.Models
{
    [Table("Payment")]
    public class Payment
    {
        [Key]
        [Column("PaymentID")]
        public int PaymentId { get; set; }

        // =========================
        // Foreign Keys
        // =========================
        [Required]
        [Column("UserID")]
        public int UserId { get; set; }

        [Required]
        [Column("SubscriptionID")]
        public int SubscriptionId { get; set; }

        [Required]
        [Column("PaymentMethodID")]
        public int PaymentMethodId { get; set; }

        // =========================
        // User input
        // =========================
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        // =========================
        // System-managed
        // =========================
        [Column(TypeName = "datetime")]
        [BindNever]
        public DateTime PaymentDate { get; set; }

        [StringLength(50)]
        [BindNever]
        public string Status { get; set; } = "Completed";

        // =========================
        // Navigation
        // =========================
        public virtual PaymentMethod PaymentMethod { get; set; } = null!;
        public virtual Subscription Subscription { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
