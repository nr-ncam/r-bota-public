using System.Numerics;

namespace Rabota;

class Menu : State
{
    Textbox seedInput;

    Dropdown worldTypeInput;
    String worldTypeDesc = "";

    Button startButton;

    Button settingsButton;
    Button exitButton;

    Vector2 UIOffset = new Vector2(10, 20);

    Texture2D logo;

    public override void Init()
    {
        base.Init();

        unsafe
        {
            Image _logo = Jarvis.LoadImage("res/r-bota.png");
            Jarvis.ImageColorReplace(&_logo, Color.White, Color.Blank);
            logo = Jarvis.LoadTextureFromImage(_logo);
            Jarvis.UnloadImage(_logo);
        }

        seedInput = new Textbox(UIOffset, 24, 6, "Seed");

        worldTypeInput = new Dropdown(new Rectangle(UIOffset.X, 90.0f + UIOffset.Y, 160.0f, 40), FetchWorldTypes(), "DEFAULT");
        worldTypeInput.ValueChanged += UpdateWorldTypeDescription;
        
        startButton = new Button(new Rectangle(175.0f + UIOffset.X, 90.0f + UIOffset.Y, 160.0f, 40), () =>
        {
            int finalSeed = 0;
            foreach (char c in seedInput.Text)
            {
                finalSeed = (finalSeed * 31) + c;
            }
            Program.ChangeState(new Game(finalSeed, worldTypeInput.Value));
        }, "Play");

        settingsButton = new Button(
            new Rectangle(
                UIOffset.X, 
                (float)(Jarvis.GetScreenHeight() - 60),
                160.0f,
                40.0f
            ),
            () =>
            {
                Program.InitSubState(new Settings());
            },
            "Settings"
        );
        exitButton = new Button(
            new Rectangle(
                UIOffset.X + 170, 
                (float)(Jarvis.GetScreenHeight() - 60),
                160.0f,
                40.0f
            ),
            () =>
            {
                Program.CloseProgram();
            },
            "Exit"
        );

        entities.Add(seedInput);
        entities.Add(worldTypeInput);
        entities.Add(startButton);
        entities.Add(settingsButton);
        entities.Add(exitButton);

        UpdateWorldTypeDescription(worldTypeInput.Value);

        Jarvis.SetWindowTitle($"R*bota | Menu");
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Draw()
    {
        Jarvis.ClearBackground(Color.Gray);

        base.Draw();

        //The separator
        Jarvis.DrawLineV(
            new Vector2(startButton.Hitbox.X + startButton.Hitbox.Width + 15, 0),
            new Vector2(startButton.Hitbox.X + startButton.Hitbox.Width + 15, Jarvis.GetScreenHeight()),
            Color.DarkGray
        );

        //World Type Description
        Jarvis.DrawTextEx(
            GameAssets.win95,
            worldTypeDesc,
            new Vector2(startButton.Hitbox.X + startButton.Hitbox.Width + 30, seedInput.Hitbox.Y),
            14,
            1,
            Color.White
        );

        //Rbota logo
        Jarvis.DrawTextureEx(
            logo,
            new Vector2(
                startButton.Hitbox.X + startButton.Hitbox.Width + 30, 
                Jarvis.GetScreenHeight() - logo.Height * 1.25f
            ),
            0,
            1,
            Color.Black
        );

        //Rbota version
        Vector2 versionTxtSize = Jarvis.MeasureTextEx(GameAssets.win95, $"v{Program.version}", 18, 1);
        Jarvis.DrawTextEx(
            GameAssets.win95,
            $"v{Program.version}",
            new Vector2(
                Jarvis.GetScreenWidth() - versionTxtSize.X * 1.25f,
                Jarvis.GetScreenHeight() - versionTxtSize.Y - 14
            ),
            18,
            1,
            Color.DarkGray
        );
    }

    public override void Unload()
    {
        base.Unload();

        Jarvis.UnloadTexture(logo);
    }

    List<String> FetchWorldTypes()
    {
        List<String> worldTypes = Directory.GetFiles("res/config").ToList();

        for(int i = 0; i < worldTypes.Count; i++)
        {
            worldTypes[i] = Path.GetFileNameWithoutExtension(worldTypes[i]).ToUpper();
        }

        return worldTypes;
    }

    void UpdateWorldTypeDescription(String worldType)
    {
        worldTypeDesc = $"--{worldType.ToUpper()}--";
        worldTypeDesc = worldTypeDesc + "\n" + WorldConfig.FetchConfig(worldType).Desc;
    }
}