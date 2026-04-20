using System.Text.Json;

namespace Rabota;

class TrackConfigJson
{
    public List<TrackConfig> tracks { get; set; }
}

class TrackConfig
{
    public String filename { get; set; }
    public String composer { get; set; }
    public bool enabled { get; set; } = true;
}

class MusicPlayer : Entity
{
    public TrackConfig curTrackInfo;
    Music curTrack;

    public MusicPlayer(String worldType)
    {
        String rawData = File.ReadAllText("res/mus/_trackConfig.json");
        TrackConfigJson jsonParsed = JsonSerializer.Deserialize<TrackConfigJson>(rawData);

        List<TrackConfig> validTracks = new List<TrackConfig>();
        foreach(TrackConfig tc in jsonParsed.tracks)
        {
            if(tc.enabled)
                validTracks.Add(tc);
        }

        int randomTrack = Random.Shared.Next(0, validTracks.Count);
        curTrack = Jarvis.LoadMusicStream($"res/mus/{validTracks[randomTrack].filename}");

        curTrackInfo = validTracks[randomTrack];

        Jarvis.SetMusicVolume(curTrack, Settings.MusicVolume / 100);

        Jarvis.PlayMusicStream(curTrack);
    }

    public override void Update()
    {
        base.Update();

        Jarvis.UpdateMusicStream(curTrack);

        if(!IsActive)
        {
            Jarvis.UnloadMusicStream(curTrack);
        }
    }
}