using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Homeix.Models
{
    [Table("CustomerPostMedia")]
    public class CustomerPostMedia
    {
        [Key]
        [Column("MediaID")]
        public int MediaId { get; set; }

        [Required]
        [Column("CustomerPostID")]
        public int CustomerPostId { get; set; }

        [Required]
        [StringLength(255)]
        public string MediaPath { get; set; } = string.Empty;

        // 🔗 Navigation

        [ValidateNever]
        [ForeignKey(nameof(CustomerPostId))]
        public virtual CustomerPost CustomerPost { get; set; } = null!;
    }
}
