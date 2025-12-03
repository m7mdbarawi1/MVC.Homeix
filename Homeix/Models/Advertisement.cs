using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Models;

[Table("Advertisement")]
public partial class Advertisement
{
    [Key]
    [Column("AdID")]
    public int AdId { get; set; }

    [Column("CreatedByUserID")]
    public int CreatedByUserId { get; set; }

    [StringLength(200)]
    public string Title { get; set; } = null!;

    [StringLength(255)]
    public string ImagePath { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("CreatedByUserId")]
    [InverseProperty("Advertisements")]
    public virtual User CreatedByUser { get; set; } = null!;
}
