using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProductManagement.EFCore.Models;

public partial class Notification
{
    [Key]
    public int Id { get; set; }

    [StringLength(450)]
    public string UserId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? SubTitle { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Date { get; set; }

    [StringLength(200)]
    public string? URL { get; set; }

    public bool Seen { get; set; }

    [StringLength(200)]
    public string Image { get; set; } = null!;
}
