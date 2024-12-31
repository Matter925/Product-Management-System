namespace ProductManagement.EFCore.Shared;
public class Lookups
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public int? OrderIndex { get; set; }
}

public class LookupsEdit
{
    public string Name { get; set; } = default!;
    public int? OrderIndex { get; set; }
}
