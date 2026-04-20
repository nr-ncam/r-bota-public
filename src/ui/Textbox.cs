using System.Numerics;

namespace Rabota;

class Textbox : Entity, IUIInteractable, IValueTracker<String>
{
    public Rectangle Hitbox
    {
        get
        {
            return _hitbox;
        }
    }
    Rectangle _hitbox;

    public bool IsSelected
    {
        get => _isSelected;
        set => _isSelected = value;
    }
    bool _isSelected = false;

    public event Action<String> ValueChanged;
    
    float deleteTimer, holdingDelete;

    float textWidth, textPadding;

    public String Text
    {
        set
        {
            curText = value;
            ValueChanged?.Invoke(curText);
        }
        get => curText;
    }
    String curText = "";
    int _defaultLimit = 10;
    int limit = -1;
    int fontSize = 18;

    String defaultText = "";

    public Textbox(Rectangle hitbox, int limit = -1, int padding = 4, String _defaultText = "")
    {
        _hitbox = hitbox;

        this.limit = limit > 0 ? limit : _defaultLimit;
        textPadding = padding;

        defaultText = _defaultText;
    }

    public Textbox(Vector2 position, int limit = -1, int padding = 4, String _defaultText = "")
    {
        this.position = position;
        this.limit = limit > 0 ? limit : _defaultLimit;
        textPadding = padding;

        string maxText = new string('W', this.limit);
        textWidth = Jarvis.MeasureTextEx(Jarvis.GetFontDefault(), maxText, fontSize, 1).X + textPadding*2;

        defaultText = _defaultText;

        _hitbox = new Rectangle(position, textWidth, 50);
    }

    public override void Update()
    {
        base.Update();

        switch(((IUIInteractable)this).CheckClick())
        {
            case UIClickState.Outside:
                IsSelected = false;
                break;
            case UIClickState.Inside:
                IsSelected = true;
                break;
            default:
                break;
        }

        if(!IsSelected)
            return;
        
        char charPressed = Convert.ToChar(Jarvis.GetCharPressed());

        if(charPressed != 0 && curText.Length < limit)
        {
            curText = curText + charPressed;
            Console.WriteLine(curText);
            ValueChanged?.Invoke(curText);
        }

        if(Jarvis.IsKeyDown(KeyboardKey.Backspace))
        {
            holdingDelete += Jarvis.GetFrameTime();
        }
        else
        {
            holdingDelete = 0f;
        }

        if(deleteTimer <= 0)
        {
            if(Jarvis.IsKeyDown(KeyboardKey.Backspace))
            {
                if(curText.Length < 1)
                    return;
                
                curText = curText.Remove(curText.Length - 1, 1);
                ValueChanged?.Invoke(curText);
                //Console.WriteLine(curText);

                if(holdingDelete <= 1)
                    deleteTimer = 0.04f;
                else if(holdingDelete <= 1.5f)
                    deleteTimer = 0.02f;
                else
                    deleteTimer = 0.002f;
            }
        }
        else
        {
            deleteTimer -= Jarvis.GetFrameTime();
        }
    }

    public override void Draw()
    {
        base.Draw();

        Jarvis.DrawRectangleRec(Hitbox, IsSelected ? new Color(255, 255, 255, 255) : new Color(255, 255, 255, 127));

        if(defaultText != "" && curText.Length < 1)
            Jarvis.DrawTextEx(
                GameAssets.win95, 
                defaultText, 
                new Vector2(Hitbox.X + textPadding, Hitbox.Y), 
                fontSize, 1, 
                Color.Gray
            );

        Jarvis.DrawTextEx(
            GameAssets.win95, 
            curText, 
            new Vector2(Hitbox.X + textPadding, Hitbox.Y), 
            fontSize, 1, 
            Color.Black
        );
    }
}