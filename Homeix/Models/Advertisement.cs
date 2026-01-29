using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Homeix.Models
{
    [Table("Advertisement")]
    public class Advertisement
    {
        [Key]
        [Column("AdID")]
        public int AdId { get; set; }

        [Column("CreatedByUserID")]
        public int? CreatedByUserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(255)]
        public string ImagePath { get; set; } = string.Empty;

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public virtual User? CreatedByUser { get; set; }
    }
}
