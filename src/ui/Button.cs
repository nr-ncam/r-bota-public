using System.Numerics;

namespace Rabota;

class Button : Entity, IUIInteractable
{
    public Rectangle Hitbox { get; }

    public bool IsSelected { get; set; }
    UIClickState _CheckClick => ((IUIInteractable)this).CheckClick();

    bool repeatFire;
    float holdTimer = 0;

    Action _onClick;

    String text = "";
    Vector2 textPosition;

    public Button(Rectangle hitbox, Action onClick, String text, bool _repeatFire = false)
    {
        Hitbox = hitbox;
        _onClick = onClick;
        this.text = text;

        repeatFire = _repeatFire;

        textPosition = Hitbox.Position + Hitbox.Size / 2 - Jarvis.MeasureTextEx(GameAssets.win95, text, 14, 1) / 2;
    }

    public override void Update()
    {
        base.Update();

        UIClickState clickState = _CheckClick;

        switch(clickState)
        {
            case UIClickState.Inside:
                IsSelected = true;
                break;
            case UIClickState.Release:
                IsSelected = false;
                holdTimer = 0;
                break;
            case UIClickState.ReleaseInside:
                IsSelected = false;
                _onClick();
                holdTimer = 0;
                break;
        }

        if(IsSelected && repeatFire)
        {
            holdTimer++;

            if(holdTimer > 4)
                _onClick();
        }
    }
    
    public override void Draw()
    {
        base.Draw();

        if(text != "")
        {
            Jarvis.DrawRectangleV(Hitbox.Position - Vector2.One, Hitbox.Size + Vector2.One, 
                IsSelected ? Color.DarkGray : Color.RayWhite);
            Jarvis.DrawRectangleV(Hitbox.Position, Hitbox.Size + Vector2.One, 
                IsSelected ? Color.RayWhite : Color.DarkGray);

            Jarvis.DrawRectangleRec(Hitbox, IsSelected ? Color.DarkGray : Color.Gray);

            Jarvis.DrawTextEx(GameAssets.win95, text, textPosition, 14, 1, Color.Black);
        }
    }
}