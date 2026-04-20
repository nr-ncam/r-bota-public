namespace Rabota;

enum UIClickState
{
    None,
    Outside,
    Inside,
    ReleaseInside,
    Release,
}

interface IUIInteractable
{
    Rectangle Hitbox { get; }
    bool IsSelected { get; set; }

    public UIClickState CheckClick() 
    {
        if(Jarvis.IsMouseButtonPressed(MouseButton.Left))
        {
            if (Jarvis.CheckCollisionPointRec(Jarvis.GetMousePosition(), Hitbox))
                return UIClickState.Inside;
            else 
                return UIClickState.Outside;
        }

        if(Jarvis.IsMouseButtonReleased(MouseButton.Left))
        {
            if (Jarvis.CheckCollisionPointRec(Jarvis.GetMousePosition(), Hitbox))
                return UIClickState.ReleaseInside;
            else 
                return UIClickState.Release;
        }

        return UIClickState.None;
    }
}