using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Homeix.Models
{
    public class CustomerPostMedia
    {
        [Key]
        public int MediaId { get; set; }

        [Required]
        public int CustomerPostId { get; set; }

        [ForeignKey(nameof(CustomerPostId))]
        public virtual CustomerPost CustomerPost { get; set; } = null!;

        [StringLength(255)]
        public string MediaPath { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; }
    }
}
