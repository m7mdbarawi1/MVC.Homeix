using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Homeix.Models
{
    [Table("PostMedia")]
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

       
        [StringLength(255)]
        public string MediaPath { get; set; } = string.Empty;

        [Column(TypeName = "datetime")]
        [BindNever]
        public DateTime UploadedAt { get; set; }

        // ⬇️ NOT saved in DB
        [NotMapped]
        public IFormFile? MediaFile { get; set; }
    }
}
