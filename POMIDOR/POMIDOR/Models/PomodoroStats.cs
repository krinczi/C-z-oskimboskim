using System;
using System.IO;
using System.Text.Json;

namespace POMIDOR.Models;

public class PomodoroStats
{
    public int CompletedCycles { get; set; }
    public long TotalWorkSeconds { get; set; }
    public long TotalBreakSeconds { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public static class PomodoroStore
{
    private static string Dir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "POMIDOR");
    private static string FilePath => Path.Combine(Dir, "pomodoro_stats.json");

    public static PomodoroStats Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<PomodoroStats>(json) ?? new PomodoroStats();
            }
        }
        catch { /* nie dramatyzujemy */ }
        return new PomodoroStats();
    }

    public static void Save(PomodoroStats stats)
    {
        try
        {
            Directory.CreateDirectory(Dir);
            var json = JsonSerializer.Serialize(stats, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
        catch { /* jak dysk płonie, to już i tak po nas */ }
    }
}
