using System;
using System.Collections.Generic;

namespace POMIDOR.Models
{
    public enum ReportKind { Daily, Weekly, Monthly }

    public sealed class ReportPeriod
    {
        public DateTime Start { get; }
        public DateTime End { get; }  // inclusive koniec dnia

        public ReportPeriod(DateTime start, DateTime end)
        {
            Start = start; End = end;
        }

        public static ReportPeriod For(ReportKind kind, DateTime anchorLocal)
        {
            anchorLocal = anchorLocal.Date;

            return kind switch
            {
                ReportKind.Daily => new ReportPeriod(anchorLocal, anchorLocal.AddDays(1).AddSeconds(-1)),
                ReportKind.Weekly =>
                    new ReportPeriod(
                        anchorLocal.AddDays(-(int)(anchorLocal.DayOfWeek == 0 ? 6 : (int)anchorLocal.DayOfWeek - 1)),
                        anchorLocal.AddDays(7 - (int)(anchorLocal.DayOfWeek == 0 ? 7 : (int)anchorLocal.DayOfWeek)).AddSeconds(-1)
                    ),
                ReportKind.Monthly =>
                    new ReportPeriod(
                        new DateTime(anchorLocal.Year, anchorLocal.Month, 1),
                        new DateTime(anchorLocal.Year, anchorLocal.Month, 1).AddMonths(1).AddSeconds(-1)
                    ),
                _ => throw new ArgumentOutOfRangeException(nameof(kind))
            };
        }

        public bool Contains(DateTime? local) =>
            local.HasValue && local.Value >= Start && local.Value <= End;
    }

    public sealed class DayPoint
    {
        public DateTime Day { get; set; }
        public int CompletedTasks { get; set; }
        public int PomodoroMinutes { get; set; }
    }

    public sealed class StatsResult
    {
        public ReportKind Kind { get; set; }
        public ReportPeriod Period { get; set; } = null!;

        [ReportField("Ukończone zadania", 1)]
        public int CompletedTasks { get; set; }

        [ReportField("Ukończone sub-taski", 2)]
        public int CompletedSubTasks { get; set; }

        [ReportField("Realizacja celów [%]", 3)]
        public double GoalsCompletionPct { get; set; }

        [ReportField("Pomodoro [min]", 4)]
        public int PomodoroMinutes { get; set; }

        [ReportField("Zaległe zadania", 5)]
        public int OverdueTasks { get; set; }

        public List<DayPoint> TimeSeries { get; set; } = new();
    }
}
