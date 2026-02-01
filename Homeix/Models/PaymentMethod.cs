using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Homeix.Models
{
    [Table("PaymentMethod")]
    public class PaymentMethod
    {
        [Key]
        [Column("PaymentMethodID")]
        public int PaymentMethodId { get; set; }

        [Required]
        [StringLength(50)]
        public string MethodName { get; set; } = string.Empty;

        // 🔗 Navigation
        [ValidateNever]
        public virtual ICollection<Payment> Payments { get; set; }
            = new List<Payment>();
    }
}
