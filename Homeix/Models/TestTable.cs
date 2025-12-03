using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Models;

[Table("TestTable")]
public partial class TestTable
{
    [Key]
    [Column("TestID")]
    public int TestId { get; set; }

    [StringLength(100)]
    public string? TestName { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }
}
