
namespace SVC.Core.Entities;

public class Employee(long id): Entity(id)
{
    public Employee() : this(0)
    { }
    public required string Name { get; set; }
    public DateTime EntryDate { get; set; }
    public DateTime? ExitDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    public Option<float?> Rating { get; set; }
    
    public long RoleId { get; set; }
    public long PlatoonId { get; set; }
    
    public virtual Role Role { get; set; }
    public virtual Platoon Platoon { get; set; }
}