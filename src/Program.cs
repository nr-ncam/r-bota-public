global using Raylib_cs;
global using Jarvis = Raylib_cs.Raylib;
using System.Diagnostics;

namespace Rabota;

/// <summary>
/// A small but needed prefix: i HOPE nobody sees the sheer bullshittery that is this codebase.
/// just. no.
/// </summary>
class Program
{
    public const String version = "0.0.3";
    static State state;
    static bool substateShouldClose = false;
    static event Action postSubstateClose;

    [System.STAThread]
    public static void Main()
    {
        #if DEBUG
            Console.WriteLine("DEBUG MODEE");
        #endif

        Settings.LoadFromJSON();
        
        Jarvis.InitWindow(800, 600, "R*bota");
        Jarvis.SetTargetFPS(60);
        Jarvis.InitAudioDevice();
        Jarvis.SetExitKey(KeyboardKey.Null);

        GameAssets.Load();

        DebugDisplay.Add("Version", () => version);
        DebugDisplay.Add("FPS", () => Jarvis.GetFPS());

        state = new Menu();

        state.Init();

        while(!Jarvis.WindowShouldClose())
        {
            Jarvis.BeginDrawing();

            Jarvis.ClearBackground(Color.Black);

            if(Jarvis.IsKeyPressed(KeyboardKey.F5))
                DebugDisplay.Visible = !DebugDisplay.Visible;

            TweenManager.Update();
            TimerManager.Update();

            if(state.substate == null)
                state.Update();
            state.Draw();

            if(state.substate != null)
            {
                state.substate.Update();
                state.substate.Draw();

                if(state.substate.openingSubstate)
                    state.substate.openingSubstate = false;

                if(substateShouldClose)
                {
                    state.substate.Unload();
                    state.substate = null;
                    substateShouldClose = false;
                    var tempAction = postSubstateClose;
                    postSubstateClose = null;
                    tempAction?.Invoke();
                }
            } 

            if(DebugDisplay.Visible)
                DebugDisplay.Draw();

            Jarvis.EndDrawing();
        }

        CloseProgram();
    }

    public static void ChangeState(State target_state)
    {
        if(state.substate != null)
        {
            CloseSubState();

            postSubstateClose += () => {
                _changeState(target_state);
            };
        }
        else _changeState(target_state);
    }
    static void _changeState(State targetState)
    {
        state.Unload();
        state = targetState;
        state.Init();
    }

    public static void InitSubState(State substate)
    {
        foreach(Entity e in state.Entities)
        {
            if(e is IAnimated animated)
            {
                animated.Anim.StopAnim();
            }
        }

        state.substate = substate;
        state.substate.openingSubstate = true;
        state.substate.Init();
    }

    public static void CloseSubState()
    {
        substateShouldClose = true;

        postSubstateClose += () =>
        {
            foreach(Entity e in state.Entities)
            {
                if(e is IAnimated animated)
                {
                    animated.Anim.PlayAnim();
                }
            }
        };
    }

    public static void CloseProgram()
    {
        CloseSubState();

        state.Unload();
        Jarvis.CloseAudioDevice();
        Jarvis.CloseWindow();
    }
}

