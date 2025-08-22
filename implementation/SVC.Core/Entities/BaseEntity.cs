namespace SVC.Core.Entities;

public abstract class Entity(long id) : IEquatable<Entity>
{
    public virtual long Id { get; set; } = id;

    public bool Equals(Entity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Entity)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}