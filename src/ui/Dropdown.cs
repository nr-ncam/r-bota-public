using System.Numerics;

namespace Rabota;

class Dropdown : Entity, IUIInteractable, IValueTracker<String>
{
    public event Action<String> ValueChanged;

    public Rectangle Hitbox { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => _isSelected = value;
    }
    bool _isSelected = false;

    List<String> values = new List<String>();
    List<Button> dropdownlings = new List<Button>();
    public String Value
    {
        get => values[SelectedIndex];
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set 
        {
            if(_selectedIndex == value) return;
            
            _selectedIndex = value;
            textPosition = Hitbox.Position + Hitbox.Size / 2 - Jarvis.MeasureTextEx(GameAssets.win95, values[SelectedIndex], 14, 1) / 2;
            ValueChanged?.Invoke(values[SelectedIndex]);
        }
    }
    int _selectedIndex = 0;

    Vector2 textPosition;

    public Dropdown(Rectangle _hitbox, List<String> _values, String defaultValue = "")
    {
        Hitbox = _hitbox;
        values = _values;

        SelectedIndex = 0;

        for(int i = 0; i < values.Count; i++)
        {
            int index = i;
            Rectangle hitboxling = new Rectangle(
                Hitbox.X, 
                Hitbox.Y + Hitbox.Height * (i + 1),
                Hitbox.Size
            );
            Button dropdownling = new Button(
                hitboxling, 
                () =>
                {
                    SelectedIndex = index;
                    IsSelected = false;
                },
                values[i]
            );
            dropdownlings.Add(dropdownling);
        }

        if(defaultValue != "" && values.Contains(defaultValue))
            SelectedIndex = values.IndexOf(defaultValue);
    }

    public override void Update()
    {
        base.Update();

        if(IsSelected)
            UpdateDrawDropdownlings();

        switch(((IUIInteractable)this).CheckClick())
        {
            case UIClickState.Release:
                IsSelected = false;
                break;
            case UIClickState.Inside:
                IsSelected = true;
                break;
            default:
                break;
        }
    }

    public override void Draw()
    {
        base.Draw();

        Jarvis.DrawRectangleRec(Hitbox, Color.RayWhite);
        Jarvis.DrawTextEx(GameAssets.win95, values[SelectedIndex], textPosition, 14, 1, Color.Black);
    }

    //sadge
    void UpdateDrawDropdownlings()
    {
        foreach(Button dropdownling in dropdownlings)
        {
            dropdownling.Update();
            dropdownling.Draw();
        }
    }
}