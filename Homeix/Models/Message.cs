using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Models;

[Table("Message")]
public partial class Message
{
    [Key]
    [Column("MessageID")]
    public int MessageId { get; set; }

    [Column("ConversationID")]
    public int ConversationId { get; set; }

    [Column("SenderUserID")]
    public int SenderUserId { get; set; }

    public string MessageText { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime SentAt { get; set; }

    [ForeignKey("ConversationId")]
    [InverseProperty("Messages")]
    public virtual Conversation Conversation { get; set; } = null!;

    [ForeignKey("SenderUserId")]
    [InverseProperty("Messages")]
    public virtual User SenderUser { get; set; } = null!;
}
