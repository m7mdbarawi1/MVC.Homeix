using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Homeix.Models
{
    [Table("CustomerPost")]
    public class CustomerPost
    {
        [Key]
        [Column("CustomerPostID")]
        public int CustomerPostId { get; set; }

        // =========================
        // Foreign Keys
        // =========================
        [Required]
        [Column("UserID")]
        public int UserId { get; set; }

        [Required]
        [Column("PostCategoryID")]
        public int PostCategoryId { get; set; }

        // =========================
        // User-input fields
        // =========================
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PriceRangeMin { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PriceRangeMax { get; set; }

        // =========================
        // System-controlled fields
        // =========================
        [Required]
        [StringLength(50)]
        [BindNever]                       // ✅ FIX: prevent POST validation issues
        public string Status { get; set; } = "Open";

        [Column(TypeName = "datetime")]
        [BindNever]                       // ✅ FIX
        public DateTime CreatedAt { get; set; }

        [BindNever]                       // ✅ FIX
        public bool IsActive { get; set; } = true;

        // =========================
        // Navigation properties
        // =========================
        public virtual User User { get; set; } = null!;
        public virtual PostCategory PostCategory { get; set; } = null!;

        // =========================
        // Related collections
        // =========================
        public virtual ICollection<Offer> Offers { get; set; }
            = new HashSet<Offer>();

        public virtual ICollection<JobProgress> JobProgresses { get; set; }
            = new HashSet<JobProgress>();
    }
}
