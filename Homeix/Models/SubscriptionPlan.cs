using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(1, 3650)]
        public int DurationDays { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Range(1, 1000)]
        public int? MaxPostsPerMonth { get; set; }

        // 🔗 Navigation
        [ValidateNever]
        public virtual ICollection<Subscription> Subscriptions { get; set; }
            = new List<Subscription>();
    }
}
