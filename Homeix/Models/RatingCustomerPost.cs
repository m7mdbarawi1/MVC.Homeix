using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Models;

[Table("RatingCustomerPost")]
public partial class RatingCustomerPost
{
    [Key]
    [Column("RatingCustomerPostID")]
    public int RatingCustomerPostId { get; set; }

    [Column("JobProgressID")]
    public int JobProgressId { get; set; }

    [Column("RaterUserID")]
    public int RaterUserId { get; set; }

    [Column("RatedUserID")]
    public int RatedUserId { get; set; }

    public int RatingValue { get; set; }

    public string? Review { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("JobProgressId")]
    [InverseProperty("RatingCustomerPosts")]
    public virtual JobProgress JobProgress { get; set; } = null!;

    [ForeignKey("RatedUserId")]
    [InverseProperty("RatingCustomerPostRatedUsers")]
    public virtual User RatedUser { get; set; } = null!;

    [ForeignKey("RaterUserId")]
    [InverseProperty("RatingCustomerPostRaterUsers")]
    public virtual User RaterUser { get; set; } = null!;
}
