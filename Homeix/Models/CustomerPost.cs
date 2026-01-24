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

        [Required]
        [StringLength(50)]
        [BindNever]
        public string Status { get; set; } = "Open";

        [Column(TypeName = "datetime")]
        [BindNever]
        public DateTime CreatedAt { get; set; }

        [BindNever]
        public bool IsActive { get; set; } = true;

        public virtual User? User { get; set; }
        public virtual PostCategory? PostCategory { get; set; }

        public virtual ICollection<Offer> Offers { get; set; } = new HashSet<Offer>();

        public virtual ICollection<JobProgress> JobProgresses { get; set; } = new HashSet<JobProgress>();
    }
}
