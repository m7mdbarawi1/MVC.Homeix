using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Models;

[Table("Offer")]
public partial class Offer
{
    [Key]
    [Column("OfferID")]
    public int OfferId { get; set; }

    [Column("CustomerPostID")]
    public int CustomerPostId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal ProposedPrice { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("CustomerPostId")]
    [InverseProperty("Offers")]
    public virtual CustomerPost CustomerPost { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Offers")]
    public virtual User User { get; set; } = null!;
}
