using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homeix.Models
{
    [Table("FavoriteWorkerPost")]
    public class FavoriteWorkerPost
    {
        [Key]
        public int FavoriteWorkerPostId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int WorkerPostId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
