namespace SVC.Core.Entities;

public class Platoon: Entity
{
    public Platoon() : base(0)
    { }

    public Platoon(long id) : base(id)
    { }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
}