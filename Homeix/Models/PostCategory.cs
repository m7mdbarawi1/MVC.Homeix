using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homeix.Models
{
    [Table("PostCategory")]
    public class PostCategory
    {
        [Key]
        [Column("PostCategoryID")]
        public int PostCategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        public virtual ICollection<CustomerPost> CustomerPosts { get; set; } = new List<CustomerPost>();

        public virtual ICollection<WorkerPost> WorkerPosts { get; set; } = new List<WorkerPost>();
    }
}
