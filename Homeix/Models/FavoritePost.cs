using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Models;

[Table("FavoritePost")]
public partial class FavoritePost
{
    [Key]
    [Column("FavoritePostID")]
    public int FavoritePostId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [StringLength(20)]
    public string? PostType { get; set; }

    [Column("PostID")]
    public int PostId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime AddedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("FavoritePosts")]
    public virtual User User { get; set; } = null!;
}
