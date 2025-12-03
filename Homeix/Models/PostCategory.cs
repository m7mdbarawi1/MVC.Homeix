using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Models;

[Table("PostCategory")]
public partial class PostCategory
{
    [Key]
    [Column("PostCategoryID")]
    public int PostCategoryId { get; set; }

    [StringLength(100)]
    public string CategoryName { get; set; } = null!;

    [InverseProperty("PostCategory")]
    public virtual ICollection<CustomerPost> CustomerPosts { get; set; } = new List<CustomerPost>();

    [InverseProperty("PostCategory")]
    public virtual ICollection<WorkerPost> WorkerPosts { get; set; } = new List<WorkerPost>();
}
