using System.Numerics;
using System.Text.Json;

namespace Rabota;

class SettingsDTO
{
    public float Volume { get; set; }
    public float MusicVolume { get; set; }
}

class Settings : State
{
    public static float Volume = 100;
    Stepper volumeStepper;

    public static float MusicVolume = 50;
    Stepper musicVolumeStepper;

    public override void Init()
    {
        base.Init();

        Vector2 settingsTxtSize = Jarvis.MeasureTextEx(
            GameAssets.win95,
            "SETTINGS",
            36, 1
        ) * 1.25f;

        volumeStepper = new Stepper(
            0, 100, 1, Volume, 
            new Rectangle(20, 20 + settingsTxtSize.Y, 120, 30),
            StepperButtonPos.Right,
            "Master Volume"
        );
        volumeStepper.ValueChanged += (float f) =>
        {
            Volume = ((int)f);
            Jarvis.SetMasterVolume(Volume);
        };

        musicVolumeStepper = new Stepper(
            0, 100, 1, MusicVolume, 
            new Rectangle(20, 100 + settingsTxtSize.Y, 120, 30),
            StepperButtonPos.Right,
            "Music Volume"
        );
        musicVolumeStepper.ValueChanged += (float f) =>
        {
            MusicVolume = ((int)f);
        };

        OnSubstateClose += SaveAndClose;

        entities.Add(volumeStepper);
        entities.Add(musicVolumeStepper);
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

        Jarvis.DrawTextEx(
            GameAssets.win95,
            "SETTINGS",
            new Vector2(20, 20),
            36, 1,
            Color.White
        );

        base.Draw();
    }

    public static void LoadFromJSON()
    {
        if(!File.Exists("settings.json")) return;

        SettingsDTO data = JsonSerializer.Deserialize<SettingsDTO>(File.ReadAllText("settings.json"));

        Volume = data.Volume;
        MusicVolume = data.MusicVolume;
    }

    void SaveAndClose()
    {
        SettingsDTO data = new SettingsDTO();

        data.Volume = Volume;
        data.MusicVolume = MusicVolume;

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        String jsonified = JsonSerializer.Serialize<SettingsDTO>(data, options);

        File.WriteAllText("settings.json", jsonified);

        Program.CloseSubState();
    }
}