using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Homeix.Models
{
    [Table("Offer")]
    public class Offer
    {
        [Key]
        [Column("OfferID")]
        public int OfferId { get; set; }

        // =========================
        // Foreign keys
        // =========================
        [Required]
        [Column("CustomerPostID")]
        public int CustomerPostId { get; set; }

        [Required]
        [Column("UserID")]
        public int UserId { get; set; }

        // =========================
        // User input
        // =========================
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal ProposedPrice { get; set; }

        // =========================
        // System-managed
        // =========================
        [Required]
        [StringLength(50)]
        [BindNever]
        public string Status { get; set; } = "Pending";

        [Column(TypeName = "datetime")]
        [BindNever]
        public DateTime CreatedAt { get; set; }

        // =========================
        // Navigation
        // =========================
        public virtual CustomerPost? CustomerPost { get; set; }
        public virtual User? User { get; set; }
    }
}
