using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Homeix.Models
{
    [Table("Advertisement")]
    public class Advertisement
    {
        [Key]
        [Column("AdID")]
        public int AdId { get; set; }

        [Column("CreatedByUserID")]
        [BindNever] // ✅ protect from overposting
        public int? CreatedByUserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [BindNever] // ✅ set only by server after upload
        public string ImagePath { get; set; } = string.Empty;

        // ❌ Not stored in DB
        [NotMapped]
        [ValidateNever] // ✅ handled manually
        public IFormFile? ImageFile { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime EndDate { get; set; }

        [Required]
        public bool IsActive { get; set; }

        // 🔗 Navigation
        [ValidateNever]
        [ForeignKey(nameof(CreatedByUserId))]
        public virtual User? CreatedByUser { get; set; }
    }
}
