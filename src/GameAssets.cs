namespace Rabota;

class GameAssets
{
    public static Font win95;
    public static Font arial;
    
    public static void Load()
    {
        win95 = Jarvis.LoadFont("res/windows 95 font.otf");
        arial = Jarvis.LoadFont("res/arialmt.ttf");
    }

    public static void Unload()
    {
        Jarvis.UnloadFont(win95);
        Jarvis.UnloadFont(arial);
    }
}