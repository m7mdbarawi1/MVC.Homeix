using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Models;

[Table("JobProgress")]
public partial class JobProgress
{
    [Key]
    [Column("JobProgressID")]
    public int JobProgressId { get; set; }

    [Column("CustomerPostID")]
    public int CustomerPostId { get; set; }

    [Column("RequestedByUserID")]
    public int RequestedByUserId { get; set; }

    [Column("AssignedToUserID")]
    public int AssignedToUserId { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime StartedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CompletedAt { get; set; }

    public bool IsRatedByCustomer { get; set; }

    public bool IsRatedByWorker { get; set; }

    [ForeignKey("AssignedToUserId")]
    [InverseProperty("JobProgressAssignedToUsers")]
    public virtual User AssignedToUser { get; set; } = null!;

    [ForeignKey("CustomerPostId")]
    [InverseProperty("JobProgresses")]
    public virtual CustomerPost CustomerPost { get; set; } = null!;

    [InverseProperty("JobProgress")]
    public virtual ICollection<RatingCustomerPost> RatingCustomerPosts { get; set; } = new List<RatingCustomerPost>();

    [InverseProperty("JobProgress")]
    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    [ForeignKey("RequestedByUserId")]
    [InverseProperty("JobProgressRequestedByUsers")]
    public virtual User RequestedByUser { get; set; } = null!;
}
