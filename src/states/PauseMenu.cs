using System.Numerics;

namespace Rabota;

class PauseMenu : State
{
    Vector2 pausedTxtSize;

    Button resumeButt;
    Button menuButt;
    Button exitButt;

    public override void Init()
    {
        base.Init();

        pausedTxtSize = Jarvis.MeasureTextEx(
            GameAssets.win95,
            "PAUSED",
            32, 1
        );

        resumeButt = new Button(
            new Rectangle(
                new Vector2(
                    Jarvis.GetScreenCenter().X - pausedTxtSize.X / 2,
                    Jarvis.GetScreenCenter().Y + 20
                ),
                pausedTxtSize
            ),
            () =>
            {
                Program.CloseSubState();
            },
            "Resume"
        );
        menuButt = new Button(
            new Rectangle(
                new Vector2(
                    Jarvis.GetScreenCenter().X - pausedTxtSize.X / 2,
                    resumeButt.Hitbox.Y + pausedTxtSize.Y + 20
                ),
                pausedTxtSize
            ),
            () =>
            {
                Program.ChangeState(new Menu());
            },
            "Exit to menu"
        );
        exitButt = new Button(
            new Rectangle(
                new Vector2(
                    Jarvis.GetScreenCenter().X - pausedTxtSize.X / 2,
                    menuButt.Hitbox.Y + pausedTxtSize.Y + 20
                ),
                pausedTxtSize
            ),
            () =>
            {
                Program.CloseProgram();
            },
            "Exit to desktop"
        );

        entities.Add(resumeButt);
        entities.Add(menuButt);
        entities.Add(exitButt);
    }

    public override void Update()
    {
        base.Update();

        CloseSubstateWithEscape();
    }

    public override void Draw()
    {
        Jarvis.DrawRectangleV(
            Vector2.Zero,
            new Vector2(Jarvis.GetScreenWidth(), Jarvis.GetScreenHeight()),
            new Color(0,0,0,200)
        );

        base.Draw();
        
        Jarvis.DrawTextEx(
            GameAssets.win95,
            "PAUSED",
            new Vector2(
                Jarvis.GetScreenCenter().X - pausedTxtSize.X / 2,
                Jarvis.GetScreenCenter().Y * 0.67f
            ),
            32, 1,
            Color.White
        );
    }
}