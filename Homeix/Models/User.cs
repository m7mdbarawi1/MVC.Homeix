using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Models;

[Table("User")]
[Index("Email", Name = "UQ__User__A9D105348926F4A1", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("UserID")]
    public int UserId { get; set; }

    [Column("RoleID")]
    public int? RoleId { get; set; }

    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [StringLength(100)]
    public string Email { get; set; } = null!;

    [StringLength(20)]
    public string PhoneNumber { get; set; } = null!;

    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(255)]
    public string? ProfilePicture { get; set; }

    // =========================
    // NAVIGATION PROPERTIES
    // =========================

    [InverseProperty("CreatedByUser")]
    public virtual ICollection<Advertisement> Advertisements { get; set; }
        = new List<Advertisement>();

    [InverseProperty("User1")]
    public virtual ICollection<Conversation> ConversationUser1s { get; set; }
        = new List<Conversation>();

    [InverseProperty("User2")]
    public virtual ICollection<Conversation> ConversationUser2s { get; set; }
        = new List<Conversation>();

    [InverseProperty("User")]
    public virtual ICollection<CustomerPost> CustomerPosts { get; set; }
        = new List<CustomerPost>();

    // ✅ NEW FAVORITES (REPLACEMENT)
    [InverseProperty("User")]
    public virtual ICollection<FavoriteWorkerPost> FavoriteWorkerPosts { get; set; }
        = new List<FavoriteWorkerPost>();

    [InverseProperty("User")]
    public virtual ICollection<FavoriteCustomerPost> FavoriteCustomerPosts { get; set; }
        = new List<FavoriteCustomerPost>();

    [InverseProperty("SenderUser")]
    public virtual ICollection<Message> Messages { get; set; }
        = new List<Message>();

    [InverseProperty("User")]
    public virtual ICollection<Payment> Payments { get; set; }
        = new List<Payment>();

    [InverseProperty("RatedUser")]
    public virtual ICollection<Rating> RatingRatedUsers { get; set; }
        = new List<Rating>();

    [InverseProperty("RaterUser")]
    public virtual ICollection<Rating> RatingRaterUsers { get; set; }
        = new List<Rating>();

    [ForeignKey("RoleId")]
    [InverseProperty("Users")]
    public virtual UserRole? Role { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Subscription> Subscriptions { get; set; }
        = new List<Subscription>();

    [InverseProperty("User")]
    public virtual ICollection<WorkerApproval> WorkerApprovalUsers { get; set; }
        = new List<WorkerApproval>();

    [InverseProperty("User")]
    public virtual ICollection<WorkerPost> WorkerPosts { get; set; }
        = new List<WorkerPost>();
}
