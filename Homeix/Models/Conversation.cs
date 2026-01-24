using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homeix.Models
{
    [Table("Conversation")]
    public class Conversation
    {
        [Key]
        [Column("ConversationID")]
        public int ConversationId { get; set; }

        [Required]
        [Column("User1ID")]
        public int User1Id { get; set; }

        [Required]
        [Column("User2ID")]
        public int User2Id { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(User1Id))]
        public virtual User? User1 { get; set; }

        [ForeignKey(nameof(User2Id))]
        public virtual User? User2 { get; set; }

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
