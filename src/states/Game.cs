using System.Numerics;

namespace Rabota;

enum GameState
{
    Countdown,
    Run,
    End,
    Gameover
}

class InfoPopup : Entity
{
    public Vector2 TextSize;
    public float BGPaddingMultiplier = 1.2f;

    Rectangle BG;

    String text;
    int fontSize;

    public InfoPopup(String _text, int _fontSize = 24)
    {
        text = _text;
        fontSize = _fontSize;

        TextSize = Jarvis.MeasureTextEx(
            GameAssets.win95,
            text,
            fontSize,
            1
        );

        position = new Vector2(
            Jarvis.GetScreenCenter().X - TextSize.X * BGPaddingMultiplier / 2,
            0
        );

        BG = new Rectangle (
            position,
            TextSize * BGPaddingMultiplier
        );
    }

    public override void Update()
    {
        base.Update();

        if(BG.Position != position)
            BG.Position = position;
    }

    public override void Draw()
    {
        base.Draw();

        Jarvis.DrawRectangleRec(
            BG,
            new Color(0,0,0,127)
        );

        Jarvis.DrawTextEx(
            GameAssets.win95,
            text,
            BG.Position + BG.Size / 2 - TextSize / 2,
            fontSize, 1,
            Color.White
        );
    }
}

class Game : State
{
    public static int CurSeed = 0;

    public static int pick_collected = 0;
    public static int pick_total;

    GameState gameState = GameState.Countdown;

    MusicPlayer musicPlayer;

    float countdownTimer = 10;

    float runTimer = 0;
    String timeLeft;
    
    World world;
    Camera2D camera;
    float cameraLag = 0.08f;

    List<Texture2D> gameoverImages = new List<Texture2D>();
    int gameoverImage = 0;
    float gameoverFrame = 0;

    static InfoPopup curPopup;

    public Game(int seed, String worldType)
    {
        Game.CurSeed = seed;

        Worldgen worldgen = new Worldgen();
        worldgen.Config(worldType, seed);
        world = worldgen.Generate();

        runTimer = WorldConfig.FetchConfig(worldType).Time;

        musicPlayer = new MusicPlayer(worldType);

        entities.Add(world.player);

        DebugDisplay.Add("World config", () => $"{worldType}, {seed}");
    }

    public override void Init()
    {
        camera = new Camera2D();
        camera.Zoom = 2;
        camera.Offset = Jarvis.GetScreenCenter();
        camera.Offset.X -= Tile.TileSize / 1;
        camera.Offset.Y -= Tile.TileSize / 1;

        //Load every image in the folder
        foreach(String imgPath in Directory.GetFiles("res/gameover/"))
        {
            Image img = Jarvis.LoadImage(imgPath);
            gameoverImages.Add(Jarvis.LoadTextureFromImage(img));
        }

        DebugDisplay.Add("Time", () => $"{MathF.Round(runTimer, 2)}");
        DebugDisplay.Add("Pickables", () => $"{pick_collected}/{pick_total}");
    }

    public override void Update()
    {
        switch(gameState)
        {
            case GameState.Countdown:
                countdownTimer -= Jarvis.GetFrameTime();

                if(countdownTimer <= 0)
                {
                    gameState = GameState.Run;
                    String trackComposer = musicPlayer.curTrackInfo.composer;
                    String trackName = musicPlayer.curTrackInfo.filename.Split(".")[0];
                    Game.CueInfoPopup(new InfoPopup($"Now Playing:\n{trackName} - {trackComposer}"));
                }
                
                Jarvis.SetWindowTitle($"R*bota | Map your route");
                break;
            case GameState.Run:
                Jarvis.SetWindowTitle($"R*bota | {timeLeft} | {world.WorldType} | {world.Seed}");

                if(Jarvis.IsKeyPressed(KeyboardKey.Escape))
                {
                    Program.InitSubState(new PauseMenu());
                }

                world.Update();
                world.player.Update(world);

                musicPlayer.Update();

                Vector2 cameraTarget = world.player.position;
                    cameraTarget = Vector2.Clamp(
                    cameraTarget, 
                    Vector2.One + Jarvis.GetScreenCenter() / camera.Zoom, 
                    (world.GetSize() - Vector2.One) * Tile.TileSize - Jarvis.GetScreenCenter() / camera.Zoom
                );
                camera.Target = Vector2.Lerp(camera.Target, cameraTarget, cameraLag);

                runTimer -= Jarvis.GetFrameTime();
                if(runTimer <= 0) TimeOver();

                TimeSpan t = TimeSpan.FromSeconds(float.Round(runTimer));
                timeLeft = String.Format("{0}:{1:D2}", t.Minutes, t.Seconds);
                break;
            case GameState.Gameover:
                gameoverFrame++;

                if(gameoverFrame == 6)
                {
                    gameoverImage = Random.Shared.Next(0, gameoverImages.Count);
                    gameoverFrame = 0;
                }

                if(Jarvis.GetKeyPressed() != 0 || Jarvis.IsMouseButtonPressed(MouseButton.Left))
                    Program.ChangeState(new Menu());
                break;
            default:
                //what
                break;
        }

        if(curPopup != null)
            curPopup.Update();
    }

    public override void Draw()
    {
        switch(gameState)
        {
            case GameState.Countdown:
                world.DrawMini();

                Jarvis.DrawTextEx(
                    GameAssets.win95,
                    "MAP YOUR ROUTE",
                    new Vector2(
                        Jarvis.GetScreenHeight() + 8,
                        0
                    ),
                    20,
                    1,
                    Color.White
                );

                String _text = $"Start in:\n{String.Format("{0:F1}", countdownTimer)}s";
                Jarvis.DrawTextEx(
                    GameAssets.win95,
                    _text,
                    new Vector2(
                        Jarvis.GetScreenHeight() + 8, 
                        Jarvis.GetScreenHeight() - Jarvis.MeasureTextEx(GameAssets.win95, _text, 24, 1).Y
                    ),
                    20,
                    1,
                    Color.White
                );
                break;
            case GameState.Run:
                Jarvis.BeginMode2D(camera);

                world.Draw();
                if(world.Ready)
                {
                    world.player.Draw();
                }
                
                Jarvis.EndMode2D();

                String bottomText = $"{timeLeft} | {pick_collected}/{pick_total}";
                Vector2 bottomTextSize = Jarvis.MeasureTextEx(
                    GameAssets.win95,
                    bottomText,
                    24,
                    1
                );
                Jarvis.DrawRectangleV(
                    new Vector2(
                        Jarvis.GetScreenCenter().X - bottomTextSize.X * 1.25f / 2,
                        Jarvis.GetScreenHeight() - bottomTextSize.Y * 1.25f
                    ),
                    bottomTextSize * 1.25f,
                    new Color(0,0,0,127)
                );
                Jarvis.DrawTextEx(
                    GameAssets.win95,
                    bottomText,
                    new Vector2(
                        Jarvis.GetScreenCenter().X - bottomTextSize.X / 2,
                        Jarvis.GetScreenHeight() - bottomTextSize.Y
                    ),
                    24,
                    1,
                    Color.White
                );
                break;
            case GameState.Gameover:
                Jarvis.DrawTextureV(
                    gameoverImages[gameoverImage],
                    Jarvis.GetScreenCenter() - gameoverImages[gameoverImage].Dimensions / 2,
                    Color.White
                );
                Jarvis.DrawTextEx(
                    GameAssets.win95,
                    "Game Over",
                    new Vector2(
                        Jarvis.GetScreenCenter().X - Jarvis.MeasureTextEx(GameAssets.win95, "Game Over", 36, 1).X / 2,
                        20
                    ),
                    36,
                    1,
                    Color.White
                );

                Vector2 pressAnyTxtSize = Jarvis.MeasureTextEx(GameAssets.win95, "Press ANY to return to menu", 24, 1);
                Jarvis.DrawTextEx(
                    GameAssets.win95,
                    "Press ANY to return to menu",
                    new Vector2(
                        Jarvis.GetScreenCenter().X - pressAnyTxtSize.X / 2,
                        Jarvis.GetScreenHeight() - pressAnyTxtSize.Y - 20
                    ),
                    24, 1,
                    Color.White
                );
                break;
            default:
                break;
        }

        if(curPopup != null)
            curPopup.Draw();
    }

    void TimeOver()
    {
        gameState = GameState.Gameover;
        Jarvis.SetWindowTitle($"R*bota | Game Over");
    }

    public override void Unload()
    {
        base.Unload();

        musicPlayer.Unload();
    }

    public static void CueInfoPopup(InfoPopup popup)
    {
        curPopup = popup;

        curPopup.position.Y = -curPopup.TextSize.Y * curPopup.BGPaddingMultiplier;

        TweenManager.FromTo(-curPopup.TextSize.Y * curPopup.BGPaddingMultiplier, 0, 0.67f, (v) => curPopup.position.Y = v, TweenEase.Quad);

        TimerManager.Schedule(() =>
        {
            TweenManager.FromTo(0, -curPopup.TextSize.Y * curPopup.BGPaddingMultiplier, 0.67f, (v) => curPopup.position.Y = v, TweenEase.Quad).OnComplete = () =>
            {
                curPopup.Unload();
            };
        }, 5);
    }
}