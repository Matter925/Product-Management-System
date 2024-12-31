using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;

namespace ProductManagement.API.DTOs;

public class AuditBaseDto
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? Type { get; set; }
    public string? TableName { get; set; }
    public DateTime? DateTime { get; set; }
    public string? Role { get; set; }
    public string? Name { get; set; }
}

public class AuditDto
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? Type { get; set; }
    public string? TableName { get; set; }
    public DateTime? DateTime { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? AffectedColumns { get; set; }
    public string? PrimaryKey { get; set; }
    public string? Role { get; set; }
    public string? Name { get; set; }
}

public class LoginLogBaseDto
{
    public string? UserId { get; set; }
    public string? Role { get; set; }
    public DateTime? LoginDate { get; set; }
    public string? Ip { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
}

public class LoginLogDto
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? Role { get; set; }
    public DateTime? LoginDate { get; set; }
    public string? Ip { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
}