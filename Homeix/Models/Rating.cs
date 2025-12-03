using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Models;

[Table("Rating")]
public partial class Rating
{
    [Key]
    [Column("RatingID")]
    public int RatingId { get; set; }

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
    [InverseProperty("Ratings")]
    public virtual JobProgress JobProgress { get; set; } = null!;

    [ForeignKey("RatedUserId")]
    [InverseProperty("RatingRatedUsers")]
    public virtual User RatedUser { get; set; } = null!;

    [ForeignKey("RaterUserId")]
    [InverseProperty("RatingRaterUsers")]
    public virtual User RaterUser { get; set; } = null!;
}
