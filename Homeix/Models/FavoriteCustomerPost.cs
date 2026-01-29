using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homeix.Models
{
    [Table("FavoriteCustomerPost")]
    public class FavoriteCustomerPost
    {
        [Key]
        public int FavoriteCustomerPostId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int CustomerPostId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
