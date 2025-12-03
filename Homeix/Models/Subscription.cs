using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Models;

[Table("Subscription")]
public partial class Subscription
{
    [Key]
    [Column("SubscriptionID")]
    public int SubscriptionId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [Column("PlanID")]
    public int PlanId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;

    [InverseProperty("Subscription")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [ForeignKey("PlanId")]
    [InverseProperty("Subscriptions")]
    public virtual SubscriptionPlan Plan { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Subscriptions")]
    public virtual User User { get; set; } = null!;
}
