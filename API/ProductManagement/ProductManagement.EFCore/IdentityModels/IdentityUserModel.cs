namespace ProductManagement.EFCore.IdentityModels;
public class IdentityUserModel
{
    public string Id { get; set; } = null!;
    public int EntityId { get; set; }
    public string EntityNameEn { get; set; } = null!;
    public string EntityNameAr { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public bool EmailConfirmed { get; set; }
    public IList<string> Roles { get; set; } = null!;
    public string Role { get; set; } = null!;
}

public class ApplicationUserModel
{
    public string Id { get; set; } = default!;
    public string Email { get; set; } = default!;
    public DateTimeOffset? LockoutEnd { get; set; }
    public DateTime? LastLoginDate { get; set; }
}
