using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Homeix.Models
{
    [Table("WorkerApproval")]
    public class WorkerApproval
    {
        [Key]
        [Column("ApprovalID")]
        public int ApprovalId { get; set; }

        [Column("UserID")]
        public int? UserId { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? Notes { get; set; }

        // 🔗 Navigation
        [ValidateNever]
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
