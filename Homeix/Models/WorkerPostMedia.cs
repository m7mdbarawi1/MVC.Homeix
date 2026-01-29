using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Homeix.Models
{
    public class WorkerPostMedia
    {
        [Key]
        public int MediaId { get; set; }

        [Required]
        public int WorkerPostId { get; set; }

        [ForeignKey(nameof(WorkerPostId))]
        public virtual WorkerPost WorkerPost { get; set; } = null!;

        [StringLength(255)]
        public string MediaPath { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; }
    }

}
