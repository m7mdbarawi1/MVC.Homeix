using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Models;

[Keyless]
[Table("sondos")]
public partial class Sondo
{
    [StringLength(50)]
    [Unicode(false)]
    public string FirstName { get; set; } = null!;
}
