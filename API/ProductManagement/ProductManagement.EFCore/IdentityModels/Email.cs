namespace ProductManagement.EFCore.IdentityModels;

public class Email
{
    public int Id { get; set; }
    public string Subject { get; set; } = default!;
    public string Body { get; set; } = default!;
    public string To { get; set; } = default!;
}
public class EmailFromUser
{
    public string FirstName { get; set; } = default!;
    public string Body { get; set; } = default!;
    public string ReplyTo { get; set; } = default!;
}
