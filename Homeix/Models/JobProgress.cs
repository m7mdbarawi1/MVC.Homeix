using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Homeix.Models
{
    [Table("JobProgress")]
    public class JobProgress
    {
        [Key]
        [Column("JobProgressID")]
        public int JobProgressId { get; set; }

        // =========================
        // Foreign keys
        // =========================
        [Required]
        [Column("CustomerPostID")]
        public int CustomerPostId { get; set; }

        [Required]
        [Column("RequestedByUserID")]
        public int RequestedByUserId { get; set; }

        [Required]
        [Column("AssignedToUserID")]
        public int AssignedToUserId { get; set; }

        // =========================
        // System-controlled fields
        // =========================
        [Required]
        [StringLength(50)]
        [BindNever]
        public string Status { get; set; } = "In Progress";

        [Column(TypeName = "datetime")]
        [BindNever]
        public DateTime StartedAt { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CompletedAt { get; set; }

        public bool IsRatedByCustomer { get; set; }
        public bool IsRatedByWorker { get; set; }

        // =========================
        // Navigation
        // =========================
        public virtual User? AssignedToUser { get; set; }
        public virtual User? RequestedByUser { get; set; }
        public virtual CustomerPost? CustomerPost { get; set; }

        // =========================
        // Related collections
        // =========================
        public virtual ICollection<RatingCustomerPost> RatingCustomerPosts { get; set; }
            = new HashSet<RatingCustomerPost>();

        public virtual ICollection<Rating> Ratings { get; set; }
            = new HashSet<Rating>();
    }
}
