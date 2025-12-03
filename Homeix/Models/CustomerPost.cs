using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Models;

[Table("CustomerPost")]
public partial class CustomerPost
{
    [Key]
    [Column("CustomerPostID")]
    public int CustomerPostId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [Column("PostCategoryID")]
    public int PostCategoryId { get; set; }

    [StringLength(200)]
    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    [StringLength(200)]
    public string Location { get; set; } = null!;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? PriceRangeMin { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? PriceRangeMax { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("CustomerPost")]
    public virtual ICollection<JobProgress> JobProgresses { get; set; } = new List<JobProgress>();

    [InverseProperty("CustomerPost")]
    public virtual ICollection<Offer> Offers { get; set; } = new List<Offer>();

    [ForeignKey("PostCategoryId")]
    [InverseProperty("CustomerPosts")]
    public virtual PostCategory PostCategory { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("CustomerPosts")]
    public virtual User User { get; set; } = null!;
}
