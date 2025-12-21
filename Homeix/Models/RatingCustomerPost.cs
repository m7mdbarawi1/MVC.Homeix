using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Homeix.Models
{
    [Table("RatingCustomerPost")]
    public class RatingCustomerPost
    {
        [Key]
        [Column("RatingCustomerPostID")]
        public int RatingCustomerPostId { get; set; }

        // =========================
        // Foreign Keys
        // =========================
        [Required]
        public int JobProgressId { get; set; }

        [Required]
        public int RaterUserId { get; set; }

        [Required]
        public int RatedUserId { get; set; }

        // =========================
        // User-input
        // =========================
        [Range(1, 5)]
        public int RatingValue { get; set; }

        public string? Review { get; set; }

        // =========================
        // System field
        // =========================
        public DateTime CreatedAt { get; set; }

        // =========================
        // Navigation (❌ NEVER VALIDATE)
        // =========================
        [ValidateNever]
        public virtual JobProgress JobProgress { get; set; } = null!;

        [ValidateNever]
        public virtual User RaterUser { get; set; } = null!;

        [ValidateNever]
        public virtual User RatedUser { get; set; } = null!;
    }
}
