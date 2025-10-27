using System;
using POMIDOR.Models;

namespace POMIDOR.Services.Reporting
{
    public interface IReportStrategy
    {
        ReportKind Kind { get; }
        ReportPeriod BuildPeriod(DateTime anchorLocal);
    }

    public abstract class ReportStrategyBase : IReportStrategy
    {
        public abstract ReportKind Kind { get; }
        public abstract ReportPeriod BuildPeriod(DateTime anchorLocal);
    }

    public sealed class DailyReportStrategy : ReportStrategyBase
    {
        public override ReportKind Kind => ReportKind.Daily;
        public override ReportPeriod BuildPeriod(DateTime a) => ReportPeriod.For(ReportKind.Daily, a);
    }

    public sealed class WeeklyReportStrategy : ReportStrategyBase
    {
        public override ReportKind Kind => ReportKind.Weekly;
        public override ReportPeriod BuildPeriod(DateTime a) => ReportPeriod.For(ReportKind.Weekly, a);
    }

    public sealed class MonthlyReportStrategy : ReportStrategyBase
    {
        public override ReportKind Kind => ReportKind.Monthly;
        public override ReportPeriod BuildPeriod(DateTime a) => ReportPeriod.For(ReportKind.Monthly, a);
    }
}
