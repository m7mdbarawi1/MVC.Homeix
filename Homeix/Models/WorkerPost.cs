using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        // System-managed field
        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual PostCategory? PostCategory { get; set; }
        public virtual User? User { get; set; }
    }
}
