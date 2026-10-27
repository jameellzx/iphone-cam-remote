using System.Text.Json;

namespace CamRemote;

/// <summary>Where the pause button sits, stored as pointer steps from the phone's top-left corner.</summary>
internal sealed class RemoteSettings
{
    public int PauseStepsX { get; set; } = 20;
    public int PauseStepsY { get; set; } = 40;

    private static string FilePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "iPhoneCamRemote", "settings.json");

    public static RemoteSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<RemoteSettings>(File.ReadAllText(FilePath)) ?? new();
        }
        catch
        {
            // A broken settings file just means starting from the defaults.
        }
        return new();
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}
