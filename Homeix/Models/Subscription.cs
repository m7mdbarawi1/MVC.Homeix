using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homeix.Models
{
    [Table("Subscription")]
    public class Subscription
    {
        [Key]
        [Column("SubscriptionID")]
        public int SubscriptionId { get; set; }

        [Column("UserID")]
        public int? UserId { get; set; }

        [Required]
        [Column("PlanID")]
        public int PlanId { get; set; }

        [Required]
        [Column(TypeName = "date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Column(TypeName = "date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active";

        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        [ForeignKey(nameof(PlanId))]
        public virtual SubscriptionPlan? Plan { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
