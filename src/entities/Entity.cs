using System.Numerics;

namespace Rabota;

public abstract class Entity
{
    public bool IsActive = true;
    public Vector2 position;
    public virtual void Update() {}
    public virtual void Draw() {}
    public virtual void Unload()
    {
        IsActive = false;
    }
}