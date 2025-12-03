using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Models;

[Table("SubscriptionPlan")]
public partial class SubscriptionPlan
{
    [Key]
    [Column("PlanID")]
    public int PlanId { get; set; }

    [StringLength(100)]
    public string PlanName { get; set; } = null!;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Price { get; set; }

    public int DurationDays { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("Plan")]
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
