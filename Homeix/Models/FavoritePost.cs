using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Homeix.Models
{
    [Table("FavoritePost")]
    public class FavoritePost
    {
        [Key]
        [Column("FavoritePostID")]
        public int FavoritePostId { get; set; }

        // =========================
        // Foreign Key
        // =========================
        [Required]
        [Column("UserID")]
        public int UserId { get; set; }

        // =========================
        // User-input fields
        // =========================
        [Required]
        [StringLength(20)]
        public string PostType { get; set; } = string.Empty;

        [Required]
        [Column("PostID")]
        public int PostId { get; set; }

        // =========================
        // System-managed field
        // =========================
        [Column(TypeName = "datetime")]
        [BindNever]
        public DateTime AddedAt { get; set; }

        // =========================
        // Navigation
        // =========================
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
