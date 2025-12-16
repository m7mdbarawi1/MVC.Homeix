using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homeix.Models
{
    [Table("UserRole")]
    public class UserRole
    {
        [Key]
        [Column("RoleID")]
        public int RoleId { get; set; }

        [Required]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        // Navigation
        public virtual ICollection<User> Users { get; set; }
            = new List<User>();
    }
}
