using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using POMIDOR.Models;

namespace POMIDOR.Services
{
    public sealed class ReportService
    {
        public (string markdownPath, string csvPath) GenerateFiles(StatsResult stats)
        {
            Directory.CreateDirectory(AppPaths.ReportsDir);

            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            string baseName = $"{stats.Kind}_{stats.Period.Start:yyyyMMdd}_{stats.Period.End:yyyyMMdd}_{stamp}";
            string mdPath = Path.Combine(AppPaths.ReportsDir, $"{baseName}.md");
            string csvPath = Path.Combine(AppPaths.ReportsDir, $"{baseName}.csv");

            File.WriteAllText(mdPath, BuildMarkdown(stats), Encoding.UTF8);
            File.WriteAllText(csvPath, BuildCsv(stats), Encoding.UTF8);

            return (mdPath, csvPath);
        }

        private static string BuildMarkdown(StatsResult s)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# Raport {s.Kind} ({s.Period.Start:yyyy-MM-dd} — {s.Period.End:yyyy-MM-dd})");
            sb.AppendLine();

            foreach (var (label, val) in GetSummary(s))
                sb.AppendLine($"- {label}: **{val}**");

            sb.AppendLine();
            sb.AppendLine("## Historia dzienna");
            sb.AppendLine("| Dzień | Ukończone | Pomodoro [min] |");
            sb.AppendLine("|---|---:|---:|");
            foreach (var p in s.TimeSeries.OrderBy(x => x.Day))
                sb.AppendLine($"| {p.Day:yyyy-MM-dd} | {p.CompletedTasks} | {p.PomodoroMinutes} |");
            return sb.ToString();
        }

        private static string BuildCsv(StatsResult s)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Day,CompletedTasks,PomodoroMinutes");
            foreach (var p in s.TimeSeries.OrderBy(x => x.Day))
                sb.AppendLine($"{p.Day:yyyy-MM-dd},{p.CompletedTasks},{p.PomodoroMinutes}");
            return sb.ToString();
        }

        private static (string Label, object? Value)[] GetSummary(StatsResult s)
        {
            return typeof(StatsResult).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(p => (Prop: p, Attr: p.GetCustomAttribute<ReportFieldAttribute>()))
                .Where(x => x.Attr != null)
                .OrderBy(x => x.Attr!.Order)
                .Select(x => (x.Attr!.Label, x.Prop.GetValue(s)))
                .ToArray();
        }
    }
}
