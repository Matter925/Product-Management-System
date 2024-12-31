using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProductManagement.EFCore.Models;

public partial class LoginLog
{
    [Key]
    public int Id { get; set; }

    [Unicode(false)]
    public string? UserId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Role { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LoginDate { get; set; }

    [Unicode(false)]
    public string? Ip { get; set; }

    [Unicode(false)]
    public string? Email { get; set; }

    [Unicode(false)]
    public string? Name { get; set; }
}
