using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homeix.Models
{
    [Table("SubscriptionPlan")]
    public class SubscriptionPlan
    {
        [Key]
        [Column("PlanID")]
        public int PlanId { get; set; }

        [Required]
        [StringLength(100)]
        public string PlanName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(1, 3650)]
        public int DurationDays { get; set; }

        // ⭐ NEW: Post limit per month
        // NULL = Unlimited posts
        [Range(1, 1000)]
        public int? MaxPostsPerMonth { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<Subscription> Subscriptions { get; set; }
            = new List<Subscription>();
    }
}
