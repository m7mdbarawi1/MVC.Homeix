using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Homeix.Models
{
    [Table("WorkerPost")]
    public class WorkerPost
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

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PriceRangeMin { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PriceRangeMax { get; set; }

        [Required]
        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }

        // 🔗 Navigation Properties

        [ValidateNever]
        public virtual User User { get; set; } = null!;

        [ValidateNever]
        public virtual PostCategory PostCategory { get; set; } = null!;

        [ValidateNever]
        public virtual ICollection<WorkerPostMedia> Media { get; set; } = new List<WorkerPostMedia>();
    }
}
