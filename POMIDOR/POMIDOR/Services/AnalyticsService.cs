using System;
using System.Collections.Generic;
using System.Linq;
using POMIDOR.Models;
using POMIDOR.Services.Reporting;

namespace POMIDOR.Services
{
    public sealed class AnalyticsService
    {
        // Wersja "klasyczna"
        public StatsResult Compute(
            IEnumerable<TodoItem> tasks,
            IEnumerable<GoalItem> goals,
            IEnumerable<PomodoroSession> sessions,
            ReportKind kind,
            DateTime anchorLocal)
            => ComputeCore(tasks, goals, sessions, ReportPeriod.For(kind, anchorLocal));

        // Wersja polimorficzna
        public StatsResult Compute(
            IEnumerable<TodoItem> tasks,
            IEnumerable<GoalItem> goals,
            IEnumerable<PomodoroSession> sessions,
            IReportStrategy strategy,
            DateTime anchorLocal)
            => ComputeCore(tasks, goals, sessions, strategy.BuildPeriod(anchorLocal));

        private static StatsResult ComputeCore(
            IEnumerable<TodoItem> tasks,
            IEnumerable<GoalItem> goals,
            IEnumerable<PomodoroSession> sessions,
            ReportPeriod period)
        {
            // Zakładamy, że Due istnieje; CompletedAt może nie istnieć, więc filtrujemy po Due.
            var inPeriod = tasks.Where(t => t.Due.HasValue && period.Contains(t.Due.Value)).ToList();

            var completedInPeriod = inPeriod.Where(t => t.IsCompleted).ToList();

            var completedSubs = inPeriod
                .SelectMany(t => t.SubTasks ?? Enumerable.Empty<SubTask>())
                .Count(st => st.IsCompleted);

            var pomoMinutes = sessions
                .Where(s => s.EndUtc.ToLocalTime() >= period.Start && s.EndUtc.ToLocalTime() <= period.End)
                .Sum(s => s.DurationMinutes);

            var overdue = tasks
                .Where(t => !t.IsCompleted && t.Due.HasValue && t.Due.Value.Date < period.End.Date)
                .Count();

            var tasksWithGoalsInPeriod = inPeriod
                .Where(t => !string.IsNullOrWhiteSpace(t.Goal))
                .ToList();

            double goalsPct = 0;
            if (tasksWithGoalsInPeriod.Count > 0)
            {
                var grouped = tasksWithGoalsInPeriod
                    .GroupBy(t => t.Goal!.Trim())
                    .Select(g =>
                    {
                        var total = g.Count();
                        var done = g.Count(t => t.IsCompleted);
                        return (total, done);
                    }).ToList();

                var totalAll = grouped.Sum(x => x.total);
                var doneAll = grouped.Sum(x => x.done);
                goalsPct = totalAll == 0 ? 0 : (double)doneAll / totalAll * 100.0;
            }

            var days = Enumerable.Range(0, (period.End.Date - period.Start.Date).Days + 1)
                                 .Select(i => period.Start.Date.AddDays(i))
                                 .ToList();

            var completedPerDay = completedInPeriod
                .Where(t => t.Due.HasValue)
                .GroupBy(t => t.Due!.Value.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var pomoPerDay = sessions
                .Where(s => s.EndUtc.ToLocalTime() >= period.Start && s.EndUtc.ToLocalTime() <= period.End)
                .GroupBy(s => s.EndUtc.ToLocalTime().Date)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.DurationMinutes));

            var series = days.Select(d => new DayPoint
            {
                Day = d,
                CompletedTasks = completedPerDay.TryGetValue(d, out var c) ? c : 0,
                PomodoroMinutes = pomoPerDay.TryGetValue(d, out var p) ? p : 0
            }).ToList();

            return new StatsResult
            {
                Kind = InferKind(period),
                Period = period,
                CompletedTasks = completedInPeriod.Count,
                CompletedSubTasks = completedSubs,
                GoalsCompletionPct = Math.Round(goalsPct, 1),
                PomodoroMinutes = pomoMinutes,
                OverdueTasks = overdue,
                TimeSeries = series
            };
        }

        private static ReportKind InferKind(ReportPeriod p)
        {
            var span = (p.End.Date - p.Start.Date).TotalDays + 1;
            if (span <= 1) return ReportKind.Daily;
            if (span <= 7) return ReportKind.Weekly;
            return ReportKind.Monthly;
        }
    }
}
