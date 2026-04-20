using System.Numerics;

namespace Rabota;

class DebugDisplay
{
    public static bool Visible = false;
    static Dictionary<String, Func<string>> _items = new();

    public static void Add(string label, Func<string> valueProvider)
    {
        _items[label] = valueProvider;
    }

    public static void Add(string label, Func<int> valueProvider) => 
        Add(label, () => valueProvider().ToString());
    public static void Add(string label, Func<float> valueProvider) =>
        Add(label, () => valueProvider().ToString("0.00"));
    public static void Add(string label, Func<Vector2> valueProvider) =>
        Add(label, () => $"({valueProvider().X:0.0}, {valueProvider().Y:0.0})");

    public static void Remove(String label)
    {
        _items.Remove(label);
    }

    public static void Clear()
    {
        _items.Clear();
    }

    public static void Draw()
    {
        int curY = 0;
        int fontSize = 18;

        foreach(var item in _items)
        {
            string text = $"{item.Key}: {item.Value()}";
            Jarvis.DrawText(text, 10, curY, fontSize, Color.Green);
            curY += fontSize;
        }
    }
}