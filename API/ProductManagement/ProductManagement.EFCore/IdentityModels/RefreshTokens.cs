namespace ProductManagement.EFCore.Models;
public partial class RefreshToken
{
    public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
    public bool IsActive => RevokedOn == null && !IsExpired;
}
