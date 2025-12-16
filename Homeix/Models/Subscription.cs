using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Homeix.Models
{
    [Table("Subscription")]
    public class Subscription
    {
        [Key]
        [Column("SubscriptionID")]
        public int SubscriptionId { get; set; }

        // =========================
        // Foreign Keys (USER INPUT)
        // =========================
        [Required]
        [Column("UserID")]
        public int UserId { get; set; }

        [Required]
        [Column("PlanID")]
        public int PlanId { get; set; }

        // =========================
        // System-managed fields
        // =========================
        [Column(TypeName = "date")]
        [BindNever] // ✅ FIX
        public DateTime StartDate { get; set; }

        [Column(TypeName = "date")]
        [BindNever] // ✅ FIX
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(50)]
        [BindNever] // ✅ FIX
        public string Status { get; set; } = "Active";

        // =========================
        // Navigation
        // =========================
        public virtual ICollection<Payment> Payments { get; set; }
            = new List<Payment>();

        [ForeignKey(nameof(PlanId))]
        public virtual SubscriptionPlan Plan { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
    }
}
