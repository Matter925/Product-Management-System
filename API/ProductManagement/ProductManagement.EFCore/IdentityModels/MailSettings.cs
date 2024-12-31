namespace ProductManagement.EFCore.IdentityModels;

public class MailSettings
{
    public string ApiKey { get; set; } = default!;
    public int EmailFor { get; set; }
    public string EmailFrom { get; set; } = default!;
    public string EmailTo { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string Host { get; set; } = default!;
    public int Port { get; set; }
}