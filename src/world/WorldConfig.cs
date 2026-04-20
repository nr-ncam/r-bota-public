using System.Text.Json;

namespace Rabota;

public class WorldConfig
{
    public int Width { get; set; }
    public int Height { get; set; }

    public float Time { get; set; } = 60;

    public String Desc { get; set; } = "";

    public int DrunkAmount { get; set; }
    public int DrunkSteps { get; set; }
    public float DrunkTurnChance { get; set; }
    public float DrunkRoomChance { get; set; }

    public int PickableAmount { get; set; }
    public int PickableMinDistance { get; set; }

    public bool IsChaos { get; set; } = false;

    public static WorldConfig FetchConfig(String _worldType)
    {
        string bareText = File.ReadAllText($"res/config/{_worldType.ToLower()}.json");
        return JsonSerializer.Deserialize<WorldConfig>(bareText);
    }
}