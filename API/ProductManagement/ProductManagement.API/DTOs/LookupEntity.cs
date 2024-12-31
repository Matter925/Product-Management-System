namespace ProductManagement.API.DTOs;

public class LookupEntityDto
{
    public int? Id { get; set; }
    public string? Name { get; set; }
}

public class LookupEntityWithImageDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Image { get; set; }
}

public class LookupEntityPlusDto
{
    public int Id { get; set; }
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
}

public class LookupEntityPlusWithImageDto
{
    public int Id { get; set; }
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Image { get; set; }
}