using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Models;

[Table("Payment")]
public partial class Payment
{
    [Key]
    [Column("PaymentID")]
    public int PaymentId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [Column("SubscriptionID")]
    public int SubscriptionId { get; set; }

    [Column("PaymentMethodID")]
    public int PaymentMethodId { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime PaymentDate { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;

    [ForeignKey("PaymentMethodId")]
    [InverseProperty("Payments")]
    public virtual PaymentMethod PaymentMethod { get; set; } = null!;

    [ForeignKey("SubscriptionId")]
    [InverseProperty("Payments")]
    public virtual Subscription Subscription { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Payments")]
    public virtual User User { get; set; } = null!;
}
