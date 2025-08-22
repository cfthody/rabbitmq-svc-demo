namespace SVC.Core.Entities;

public class Role(long id) : Entity(id)
{
    public Role() : this(0)
    { }

    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
}