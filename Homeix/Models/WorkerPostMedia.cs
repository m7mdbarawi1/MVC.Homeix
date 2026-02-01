using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Homeix.Models
{
    [Table("WorkerPostMedia")]
    public class WorkerPostMedia
    {
        [Key]
        [Column("MediaID")]
        public int MediaId { get; set; }

        [Required]
        [Column("WorkerPostID")]
        public int WorkerPostId { get; set; }

        [Required]
        [StringLength(255)]
        public string MediaPath { get; set; } = string.Empty;

        // 🔗 Navigation
        [ValidateNever]
        [ForeignKey(nameof(WorkerPostId))]
        public virtual WorkerPost WorkerPost { get; set; } = null!;
    }
}
