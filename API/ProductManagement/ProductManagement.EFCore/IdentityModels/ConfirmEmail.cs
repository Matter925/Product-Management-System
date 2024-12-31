namespace ProductManagement.EFCore.IdentityModels;
public class ConfirmEmailModel
{
    public string Email { get; set; } = default!;
    public string Token { get; set; } = default!;
}

public class EmailForConfirmation
{
    public string Email { get; set; } = default!;
    public bool IsPortal { get; set; }
}