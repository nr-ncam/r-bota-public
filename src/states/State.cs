namespace Rabota;

/// <summary>
/// Primary use is through Program.ChangeState(). However, treating this as an object effectively functions as a substate
/// </summary>
class State
{
    public State substate;
    public bool openingSubstate;
    public event Action OnSubstateClose;

    public List<Entity> Entities => entities;
    protected List<Entity> entities = new List<Entity>();

    public virtual void Init() {}
    public virtual void Update()
    {
        foreach(Entity e in entities)
        {
            e.Update();
        }
    }
    public virtual void Draw()
    {
        foreach(Entity e in entities)
        {
            e.Draw();
        }
    }
    public virtual void Unload()
    {
        if(substate != null)
            substate.Unload();
        
        foreach(Entity e in entities)
            e.Unload();
    }

    public void CloseSubstateWithEscape()
    {
        if(openingSubstate) return;

        if(Jarvis.IsKeyPressed(KeyboardKey.Escape))
        {
            OnSubstateClose?.Invoke();
            Program.CloseSubState();
        }
    }
}