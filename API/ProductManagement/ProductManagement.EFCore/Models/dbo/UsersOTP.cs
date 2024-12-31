using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProductManagement.EFCore.Models;

[Table("UsersOTP")]
public partial class UsersOTP
{
    [Key]
    public int Id { get; set; }

    [StringLength(450)]
    public string UserId { get; set; } = null!;

    [StringLength(6)]
    [Unicode(false)]
    public string OTP { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime ExpiryTime { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Type { get; set; }

    [StringLength(450)]
    [Unicode(false)]
    public string? IP { get; set; }
}
