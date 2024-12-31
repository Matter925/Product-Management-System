using Microsoft.EntityFrameworkCore.ChangeTracking;

using Newtonsoft.Json;

using ProductManagement.EFCore.Enums;
using ProductManagement.EFCore.Models;

namespace ProductManagement.EFCore.Shared;
public class AuditEntry
{
    public AuditEntry(EntityEntry entry)
    {
        Entry = entry;
    }

    public EntityEntry Entry { get; }
    public string UserId { get; set; }
    public string TableName { get; set; }
    public string? Role { get; set; }
    public string? Name { get; set; }
    public Dictionary<string, object> KeyValues { get; set; } = new Dictionary<string, object>();
    public Dictionary<string, object> OldValues { get; set; } = new Dictionary<string, object>();
    public Dictionary<string, object> NewValues { get; set; } = new Dictionary<string, object>();
    public AuditType AuditType { get; set; }
    public List<string> ChangedColumns { get; set; } = new List<string>();

    public Audit ToAudit()
    {
        var audit = new Audit();
        audit.UserId = UserId;
        audit.Role = Role;
        audit.Name = Name;
        audit.Type = AuditType.ToString();
        audit.TableName = TableName;
        audit.DateTime = DateTime.UtcNow;
        if (AuditType == AuditType.Create)
            audit.PrimaryKey = null;
        else
            audit.PrimaryKey = JsonConvert.SerializeObject(KeyValues);
        audit.OldValues = OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues);
        audit.NewValues = NewValues.Count == 0 ? null : JsonConvert.SerializeObject(NewValues);
        audit.AffectedColumns = ChangedColumns.Count == 0 ? null : JsonConvert.SerializeObject(ChangedColumns);
        return audit;
    }
}
