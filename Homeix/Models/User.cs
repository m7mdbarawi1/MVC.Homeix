using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Models
{
    [Table("User")]
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        [Key]
        [Column("UserID")]
        public int UserId { get; set; }

        [Column("RoleID")]
        public int? RoleId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(255)]
        public string? ProfilePicture { get; set; }

        // =========================
        // NAVIGATION PROPERTIES
        // =========================

        [ValidateNever]
        [ForeignKey(nameof(RoleId))]
        public virtual UserRole? Role { get; set; }

        [ValidateNever]
        public virtual ICollection<Advertisement> Advertisements { get; set; }
            = new List<Advertisement>();

        [ValidateNever]
        public virtual ICollection<Conversation> ConversationUser1s { get; set; }
            = new List<Conversation>();

        [ValidateNever]
        public virtual ICollection<Conversation> ConversationUser2s { get; set; }
            = new List<Conversation>();

        [ValidateNever]
        public virtual ICollection<CustomerPost> CustomerPosts { get; set; }
            = new List<CustomerPost>();

        [ValidateNever]
        public virtual ICollection<FavoriteWorkerPost> FavoriteWorkerPosts { get; set; }
            = new List<FavoriteWorkerPost>();

        [ValidateNever]
        public virtual ICollection<FavoriteCustomerPost> FavoriteCustomerPosts { get; set; }
            = new List<FavoriteCustomerPost>();

        [ValidateNever]
        public virtual ICollection<Message> Messages { get; set; }
            = new List<Message>();

        [ValidateNever]
        public virtual ICollection<Payment> Payments { get; set; }
            = new List<Payment>();

        [ValidateNever]
        public virtual ICollection<Rating> RatingRatedUsers { get; set; }
            = new List<Rating>();

        [ValidateNever]
        public virtual ICollection<Rating> RatingRaterUsers { get; set; }
            = new List<Rating>();

        [ValidateNever]
        public virtual ICollection<Subscription> Subscriptions { get; set; }
            = new List<Subscription>();

        [ValidateNever]
        public virtual ICollection<WorkerApproval> WorkerApprovalUsers { get; set; }
            = new List<WorkerApproval>();

        [ValidateNever]
        public virtual ICollection<WorkerPost> WorkerPosts { get; set; }
            = new List<WorkerPost>();
    }
}
