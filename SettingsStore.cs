using System.Text.Json;

namespace mouse_nudge;

static class SettingsStore
{
    static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    static string DirectoryPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MouseNudge");

    static string FilePath => Path.Combine(DirectoryPath, "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return new AppSettings();
            }

            AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath));

            return settings is null ? new AppSettings() : Sanitize(settings);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or NotSupportedException or ArgumentException)
        {
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(DirectoryPath);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, SerializerOptions));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException)
        {
        }
    }

    static AppSettings Sanitize(AppSettings settings)
    {
        settings.IntervalSeconds = Math.Clamp(settings.IntervalSeconds, 1, 3600);
        settings.IntervalJitterPercent = Math.Clamp(settings.IntervalJitterPercent, 0, 50);
        settings.DistancePixels = Math.Clamp(settings.DistancePixels, 1, 100);
        settings.EdgePaddingPixels = Math.Clamp(settings.EdgePaddingPixels, 0, 500);
        settings.IdleThresholdSeconds = Math.Clamp(settings.IdleThresholdSeconds, 5, 600);

        return settings;
    }
}
