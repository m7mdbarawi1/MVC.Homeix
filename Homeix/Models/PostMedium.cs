using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Homeix.Models
{
    [Table("PostMedia")] // ✅ EXACT DB TABLE NAME
    public class PostMedium
    {
        [Key]
        [Column("MediaID")]
        public int MediaId { get; set; }

        [Required]
        [StringLength(20)]
        public string PostType { get; set; } = string.Empty;

        [Required]
        [Column("PostID")]
        public int PostId { get; set; }

        [Required]
        [StringLength(255)]
        public string MediaPath { get; set; } = string.Empty;

        [Column(TypeName = "datetime")]
        [BindNever] // system-managed
        public DateTime UploadedAt { get; set; }
    }
}
