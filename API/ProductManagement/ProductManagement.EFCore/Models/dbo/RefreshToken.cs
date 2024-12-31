using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProductManagement.EFCore.Models;

public partial class RefreshToken
{
    [Key]
    public int Id { get; set; }

    public string Token { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime ExpiresOn { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? RevokedOn { get; set; }

    [StringLength(450)]
    public string ApplicationUserId { get; set; } = null!;
}
