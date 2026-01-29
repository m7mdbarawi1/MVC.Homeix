using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homeix.Models
{
    [Table("WorkerApproval")]
    public partial class WorkerApproval
    {
        [Key]
        [Column("ApprovalID")]
        public int ApprovalId { get; set; }

        [Column("UserID")]
        public int? UserId { get; set; }

        public string? Notes { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
