using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Homeix.Models
{
    [Table("FavoriteWorkerPost")]
    public class FavoriteWorkerPost
    {
        [Key]
        [Column("FavoritePostID")]
        public int FavoritePostId { get; set; }

        [Required]
        [Column("UserID")]
        public int UserId { get; set; }

        [Required]
        [Column("WorkerPostID")]
        public int WorkerPostId { get; set; }

        // 🔗 Navigation

        [ValidateNever]
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ValidateNever]
        [ForeignKey(nameof(WorkerPostId))]
        public virtual WorkerPost WorkerPost { get; set; } = null!;
    }
}
