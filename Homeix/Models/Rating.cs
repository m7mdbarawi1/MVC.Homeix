using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Homeix.Models
{
    [Table("Rating")]
    public class Rating
    {
        [Key]
        [Column("RatingID")]
        public int RatingId { get; set; }

        // =========================
        // Foreign Keys
        // =========================
        [Required]
        [Column("JobProgressID")]
        public int JobProgressId { get; set; }

        [Required]
        [Column("RaterUserID")]
        public int RaterUserId { get; set; }

        [Required]
        [Column("RatedUserID")]
        public int RatedUserId { get; set; }

        // =========================
        // User-input fields
        // =========================
        [Range(1, 5)]
        public int RatingValue { get; set; }

        public string? Review { get; set; }

        // =========================
        // System-managed field
        // =========================
        [Column(TypeName = "datetime")]
        [BindNever]
        public DateTime CreatedAt { get; set; }

        // =========================
        // Navigation (❌ DO NOT VALIDATE)
        // =========================
        [ValidateNever]
        public virtual JobProgress JobProgress { get; set; } = null!;

        [ValidateNever]
        public virtual User RatedUser { get; set; } = null!;

        [ValidateNever]
        public virtual User RaterUser { get; set; } = null!;
    }
}
