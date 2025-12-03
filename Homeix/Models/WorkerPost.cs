using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Models;

[Table("WorkerPost")]
public partial class WorkerPost
{
    [Key]
    [Column("WorkerPostID")]
    public int WorkerPostId { get; set; }

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

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("PostCategoryId")]
    [InverseProperty("WorkerPosts")]
    public virtual PostCategory PostCategory { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("WorkerPosts")]
    public virtual User User { get; set; } = null!;
}
