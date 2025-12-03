using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Models;

[Table("Conversation")]
public partial class Conversation
{
    [Key]
    [Column("ConversationID")]
    public int ConversationId { get; set; }

    [Column("User1ID")]
    public int User1Id { get; set; }

    [Column("User2ID")]
    public int User2Id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [InverseProperty("Conversation")]
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    [ForeignKey("User1Id")]
    [InverseProperty("ConversationUser1s")]
    public virtual User User1 { get; set; } = null!;

    [ForeignKey("User2Id")]
    [InverseProperty("ConversationUser2s")]
    public virtual User User2 { get; set; } = null!;
}
