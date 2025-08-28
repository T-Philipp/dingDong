using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace dingdongwin;

public class AppSettings
{
    public string? TimesUrl { get; set; }
    public string? Mp3Path { get; set; }
    public float? Volume { get; set; } = 0.3f;
}

public static class SettingsManager
{
    private static readonly string Dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "dingDong", "windows-client");
    private static readonly string FilePath = Path.Combine(Dir, "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                var s = JsonSerializer.Deserialize(json, AppSettingsJsonContext.Default.AppSettings);
                if (s != null) return s;
            }
        }
        catch { }
    return new AppSettings();
    }

    public static void Save(this AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(Dir);
            var json = JsonSerializer.Serialize(settings, AppSettingsJsonContext.Default.AppSettings);
            File.WriteAllText(FilePath, json);
        }
        catch { }
    }
}

// System.Text.Json source generation context for NativeAOT
[JsonSourceGenerationOptions(WriteIndented = true, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(AppSettings))]
internal partial class AppSettingsJsonContext : JsonSerializerContext
{
}
