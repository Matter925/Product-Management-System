namespace ProductManagement.Shared.Extensions;
public static class StringExtensionMethods
{
    public static string LanguagePart(this string text, string lang)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var index = text.IndexOf("~");

        if (index == -1)
            return text;

        if (lang.ToLower() == "en" || lang.ToLower().StartsWith("en"))
            return text[..index].Trim();
        else
            return text[(index + 1)..].Trim();
    }
}

public static class DynamicExtensions
{
    public static string? GetLanguagePart(dynamic obj, string lang)
    {
        if (obj == null)
            return null;

        string name = obj.Name;
        if (name != null)
            return name.LanguagePart(lang);

        return null;
    }
}