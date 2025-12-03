using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Models;

public partial class PostMedium
{
    [Key]
    [Column("MediaID")]
    public int MediaId { get; set; }

    [StringLength(20)]
    public string PostType { get; set; } = null!;

    [Column("PostID")]
    public int PostId { get; set; }

    [StringLength(255)]
    public string MediaPath { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime UploadedAt { get; set; }
}
