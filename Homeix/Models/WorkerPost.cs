using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Homeix.Models
{
    [Table("WorkerPost")]
    public partial class WorkerPost
    {
        [Key]
        [Column("WorkerPostID")]
        public int WorkerPostId { get; set; }

        [Required]
        [Column("UserID")]
        public int UserId { get; set; }

        [Required]
        [Column("PostCategoryID")]
        public int PostCategoryId { get; set; }

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

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // =========================
        // Navigation
        // =========================
        [ValidateNever]
        public virtual User? User { get; set; }

        [ValidateNever]
        public virtual PostCategory? PostCategory { get; set; }

        // 🖼 MEDIA (THIS FIXES YOUR ERROR)
        [ValidateNever]
        public virtual ICollection<PostMedium> PostMedia { get; set; }
            = new List<PostMedium>();

      
    }
}
