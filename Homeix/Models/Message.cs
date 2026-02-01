using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Homeix.Models
{
    [Table("Message")]
    public class Message
    {
        [Key]
        [Column("MessageID")]
        public int MessageId { get; set; }

        [Required]
        [Column("ConversationID")]
        public int ConversationId { get; set; }

        [Column("SenderUserID")]
        public int? SenderUserId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string MessageText { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "datetime")]
        [BindNever]
        public DateTime SentAt { get; set; } = DateTime.Now;

        // 🔗 Navigation

        [ValidateNever]
        [ForeignKey(nameof(ConversationId))]
        public virtual Conversation Conversation { get; set; } = null!;

        [ValidateNever]
        [ForeignKey(nameof(SenderUserId))]
        public virtual User? SenderUser { get; set; }
    }
}
