using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Homeix.Models
{
    [Table("FavoriteCustomerPost")]
    public class FavoriteCustomerPost
    {
        [Key]
        [Column("FavoriteCustomerPostID")]
        public int FavoriteCustomerPostId { get; set; }

        [Required]
        [Column("UserID")]
        public int UserId { get; set; }

        [Required]
        [Column("CustomerPostID")]
        public int CustomerPostId { get; set; }

        // 🔗 Navigation Properties

        [ValidateNever]
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ValidateNever]
        [ForeignKey(nameof(CustomerPostId))]
        public virtual CustomerPost CustomerPost { get; set; } = null!;
    }
}
