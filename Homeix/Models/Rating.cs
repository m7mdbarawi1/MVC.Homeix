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

        [Column("RaterUserID")]
        public int? RaterUserId { get; set; }

        [Column("RatedUserID")]
        public int? RatedUserId { get; set; }

        [Required]
        [Range(1, 5)]
        public int RatingValue { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? Review { get; set; }

        [Required]
        [Column(TypeName = "datetime")]
        [BindNever]
        public DateTime CreatedAt { get; set; }

        // 🔗 Navigation

        [ValidateNever]
        [ForeignKey(nameof(RatedUserId))]
        public virtual User? RatedUser { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(RaterUserId))]
        public virtual User? RaterUser { get; set; }
    }
}
