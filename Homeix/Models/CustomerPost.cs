using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Homeix.Models
{
    [Table("CustomerPost")]
    public class CustomerPost
    {
        [Key]
        [Column("CustomerPostID")]
        public int CustomerPostId { get; set; }

        [Column("UserID")]
        public int? UserId { get; set; }

        [Column("PostCategoryID")]
        public int? PostCategoryId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PriceRangeMin { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PriceRangeMax { get; set; }

        [Required]
        [Column(TypeName = "datetime")]
        [BindNever]
        public DateTime CreatedAt { get; set; }

        // 🔗 Navigation

        [ValidateNever]
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(PostCategoryId))]
        public virtual PostCategory? PostCategory { get; set; }

        [ValidateNever]
        public virtual ICollection<CustomerPostMedia> Media { get; set; }
            = new List<CustomerPostMedia>();
    }
}
