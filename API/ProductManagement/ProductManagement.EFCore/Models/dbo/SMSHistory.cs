using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProductManagement.EFCore.Models;

[Table("SMSHistory")]
public partial class SMSHistory
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime DateTime { get; set; }

    public string Content { get; set; } = null!;

    [StringLength(255)]
    public string PhoneNo { get; set; } = null!;

    public bool State { get; set; }
}
