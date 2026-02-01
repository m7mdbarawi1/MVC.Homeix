using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Homeix.Models
{
    [Table("Conversation")]
    public class Conversation
    {
        [Key]
        [Column("ConversationID")]
        public int ConversationId { get; set; }

        [Column("User1ID")]
        public int? User1Id { get; set; }

        [Column("User2ID")]
        public int? User2Id { get; set; }

        [Required]
        [Column(TypeName = "datetime")]
        [BindNever] // ✅ protect from overposting
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 🔗 Navigation

        [ValidateNever]
        [ForeignKey(nameof(User1Id))]
        public virtual User? User1 { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(User2Id))]
        public virtual User? User2 { get; set; }

        [ValidateNever]
        public virtual ICollection<Message> Messages { get; set; }
            = new List<Message>();
    }
}
