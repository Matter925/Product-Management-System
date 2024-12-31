namespace ProductManagement.EFCore.IdentityModels;
public class ChangeEmailModel
{
    public string UserId { get; set; } = default!;

    public string NewEmail { get; set; } = default!;
}
