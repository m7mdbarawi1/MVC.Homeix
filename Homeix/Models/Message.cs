using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Homeix.Models
{
    [Table("Message")]
    public class Message
    {
        [Key]
        [Column("MessageID")]
        public int MessageId { get; set; }

        // =========================
        // Foreign keys
        // =========================
        [Required]
        [Column("ConversationID")]
        public int ConversationId { get; set; }

        [Required]
        [Column("SenderUserID")]
        public int SenderUserId { get; set; }

        // =========================
        // User input
        // =========================
        [Required]
        public string MessageText { get; set; } = string.Empty;

        // =========================
        // System-managed
        // =========================
        [Column(TypeName = "datetime")]
        [BindNever]
        public DateTime SentAt { get; set; } = DateTime.Now;

        // =========================
        // Navigation
        // =========================
        public virtual Conversation? Conversation { get; set; }
        public virtual User? SenderUser { get; set; }
    }
}
