using System.Numerics;

namespace Rabota;

enum StepperButtonPos
{
    Surround,
    Right,
    Left,
}

class Stepper : Entity, IValueTracker<float>
{
    public event Action<float> ValueChanged;

    public float Value
    {
        get => _value;
        set
        {
            float clamped = Math.Clamp(value, min, max);
            if(_value == clamped) return;

            _value = clamped;
            valueTextbox.Text = _value.ToString();
            ValueChanged?.Invoke(_value);
        }
    }
    float _value = 0;

    float min, max, step;

    String caption;

    Button decrease, increase;
    Textbox valueTextbox;

    public Stepper(
        float _min, 
        float _max, 
        float _step, 
        float defaultValue, 
        Rectangle textboxHitbox, 
        StepperButtonPos buttonPos = StepperButtonPos.Right, 
        String _caption = "")
    {
        min = _min;
        max = _max;
        step = _step;
        caption = _caption;

        if(caption != "")
        {
            Vector2 captionSize = Jarvis.MeasureTextEx(GameAssets.win95, caption, 24, 1);

            textboxHitbox.Y += captionSize.Y;
        }

        valueTextbox = new Textbox(
            textboxHitbox,
            max.ToString().Length,
            4
        );
        
        valueTextbox.ValueChanged += (string s) =>
        {
            if (float.TryParse(s, out float newVal))
            {
                Value = newVal;
            }
            else
            {
                Console.WriteLine("fam wtf is this");
                if (valueTextbox.Text.Length > 0)
                    valueTextbox.Text = valueTextbox.Text.Remove(valueTextbox.Text.Length - 1);
            }
        };

        Value = defaultValue;

        InitButtons(buttonPos);
    }

    void InitButtons(StepperButtonPos buttonPos)
    {
        Rectangle decreaseHitbox, increaseHitbox;
        switch(buttonPos)
        {
            case StepperButtonPos.Right:
                decreaseHitbox = new Rectangle(
                    valueTextbox.Hitbox.X + valueTextbox.Hitbox.Width, 
                    valueTextbox.Hitbox.Y, 
                    new Vector2(valueTextbox.Hitbox.Height)
                );
                increaseHitbox = new Rectangle(
                    decreaseHitbox.X + decreaseHitbox.Width, 
                    valueTextbox.Hitbox.Y, 
                    new Vector2(valueTextbox.Hitbox.Height)
                );
                break;
            default:
                decreaseHitbox = new Rectangle(
                    valueTextbox.Hitbox.X - valueTextbox.Hitbox.Height, 
                    valueTextbox.Hitbox.Y, 
                    new Vector2(valueTextbox.Hitbox.Height)
                );
                increaseHitbox = new Rectangle(
                    valueTextbox.Hitbox.X + valueTextbox.Hitbox.Width, 
                    valueTextbox.Hitbox.Y, 
                    new Vector2(valueTextbox.Hitbox.Height)
                );
                break;
        }

        decrease = new Button(
            decreaseHitbox,
            () =>
            {
                Value -= Jarvis.IsKeyDown(KeyboardKey.LeftShift) ? step * 10 : step;
            },
            "-",
            true
        );
        increase = new Button(
            increaseHitbox,
            () =>
            {
                Value += Jarvis.IsKeyDown(KeyboardKey.LeftShift) ? step * 10 : step;
            },
            "+",
            true
        );
    }

    public override void Update()
    {
        base.Update();

        valueTextbox.Update();
        decrease.Update();
        increase.Update();
    }

    public override void Draw()
    {
        base.Draw();

        if(caption != "")
        {
            Vector2 captionSize = Jarvis.MeasureTextEx(GameAssets.win95, caption, 24, 1);

            Jarvis.DrawTextEx(
                GameAssets.win95,
                caption,
                new Vector2(valueTextbox.Hitbox.X, valueTextbox.Hitbox.Y - captionSize.Y),
                24, 1,
                Color.White
            );
        }
        valueTextbox.Draw();
        decrease.Draw();
        increase.Draw();
    }
}