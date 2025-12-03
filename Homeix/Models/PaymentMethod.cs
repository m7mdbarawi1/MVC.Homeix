using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Models;

[Table("PaymentMethod")]
public partial class PaymentMethod
{
    [Key]
    [Column("PaymentMethodID")]
    public int PaymentMethodId { get; set; }

    [StringLength(50)]
    public string MethodName { get; set; } = null!;

    [InverseProperty("PaymentMethod")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
