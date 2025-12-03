using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Models;

[Table("WorkerApproval")]
public partial class WorkerApproval
{
    [Key]
    [Column("ApprovalID")]
    public int ApprovalId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [Column("ReviewedByUserID")]
    public int? ReviewedByUserId { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;

    public string? Notes { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime RequestedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ReviewedAt { get; set; }

    [ForeignKey("ReviewedByUserId")]
    [InverseProperty("WorkerApprovalReviewedByUsers")]
    public virtual User? ReviewedByUser { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("WorkerApprovalUsers")]
    public virtual User User { get; set; } = null!;
}
