using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProductManagement.EFCore.Models;

public partial class Audit
{
    [Key]
    public int Id { get; set; }

    public string? UserId { get; set; }

    [StringLength(50)]
    public string? Type { get; set; }

    public string? TableName { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DateTime { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? AffectedColumns { get; set; }

    public string? PrimaryKey { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Role { get; set; }

    [Unicode(false)]
    public string? Name { get; set; }
}
