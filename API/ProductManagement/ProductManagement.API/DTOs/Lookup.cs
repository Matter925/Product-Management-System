using System.ComponentModel.DataAnnotations;

namespace ProductManagement.API.DTOs;

public class LookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int? OrderIndex { get; set; }
}

public class LookupCreateDto
{
    [StringLength(100)]
    public string Name { get; set; } = null!;
    public int? OrderIndex { get; set; }
}

public class LookupUpdateDto
{
    [StringLength(100)]
    public string Name { get; set; } = null!;
    public int? OrderIndex { get; set; }
}
