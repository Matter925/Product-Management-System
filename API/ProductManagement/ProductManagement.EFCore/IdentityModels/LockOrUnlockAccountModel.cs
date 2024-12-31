namespace ProductManagement.EFCore.IdentityModels;
public class LockOrUnlockAccountModel
{
    public string UserId { get; set; } = default!;
    public bool LockAccount { get; set; }
}