using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using POMIDOR.Models;
using POMIDOR.Services;

namespace POMIDOR.ViewModels
{
    public sealed class AnalyticsViewModel : INotifyPropertyChanged
    {
        private readonly AnalyticsService _analytics = new();
        private readonly ReportService _reports = new();

        private readonly ObservableCollection<TodoItem> _tasks;
        private readonly ObservableCollection<GoalItem> _goals;

        private StatsResult? _stats;
        public StatsResult? Stats { get => _stats; private set { _stats = value; OnPropertyChanged(); RefreshPlots(); } }

        public PlotModel? CompletedTasksModel { get; private set; }
        public PlotModel? PomodoroModel { get; private set; }

        public DateTime AnchorDate { get; set; } = DateTime.Now;

        public AnalyticsViewModel(ObservableCollection<TodoItem> tasks, ObservableCollection<GoalItem> goals)
        {
            _tasks = tasks;
            _goals = goals;

            PomodoroSessionStore.SessionAppended += (_, __) => Compute(ReportKind.Weekly);
            Compute(ReportKind.Weekly);
        }

        public void Compute(ReportKind kind)
        {
            var sessions = PomodoroSessionStore.LoadAll();
            Stats = _analytics.Compute(_tasks, _goals, sessions, kind, AnchorDate);
        }

        public (string md, string csv) GenerateReportFiles()
        {
            if (Stats == null) throw new InvalidOperationException("Brak danych raportu.");
            return _reports.GenerateFiles(Stats);
        }

        private void RefreshPlots()
        {
            if (Stats == null)
            {
                CompletedTasksModel = null;
                PomodoroModel = null;
                OnPropertyChanged(nameof(CompletedTasksModel));
                OnPropertyChanged(nameof(PomodoroModel));
                return;
            }

            // Ukończone zadania (linia)
            var tasksPlot = new PlotModel { Title = "Ukończone zadania (per dzień)" };
            var tasksX = new CategoryAxis { Position = AxisPosition.Bottom };
            var tasksY = new LinearAxis { Position = AxisPosition.Left, Minimum = 0 };
            var tasksSeries = new LineSeries { MarkerType = MarkerType.Circle };

            for (int i = 0; i < Stats.TimeSeries.Count; i++)
            {
                var pt = Stats.TimeSeries[i];
                tasksX.Labels.Add(pt.Day.ToString("MM-dd"));
                tasksSeries.Points.Add(new DataPoint(i, pt.CompletedTasks));
            }
            tasksPlot.Axes.Add(tasksX);
            tasksPlot.Axes.Add(tasksY);
            tasksPlot.Series.Add(tasksSeries);
            CompletedTasksModel = tasksPlot;

            // Minuty Pomodoro (linia)
            var pomoPlot = new PlotModel { Title = "Minuty Pomodoro (per dzień)" };
            var pomoX = new CategoryAxis { Position = AxisPosition.Bottom };
            var pomoY = new LinearAxis { Position = AxisPosition.Left, Minimum = 0 };
            var pomoSeries = new LineSeries { MarkerType = MarkerType.Circle };

            for (int i = 0; i < Stats.TimeSeries.Count; i++)
            {
                var pt = Stats.TimeSeries[i];
                pomoX.Labels.Add(pt.Day.ToString("MM-dd"));
                pomoSeries.Points.Add(new DataPoint(i, pt.PomodoroMinutes));
            }
            pomoPlot.Axes.Add(pomoX);
            pomoPlot.Axes.Add(pomoY);
            pomoPlot.Series.Add(pomoSeries);
            PomodoroModel = pomoPlot;

            OnPropertyChanged(nameof(CompletedTasksModel));
            OnPropertyChanged(nameof(PomodoroModel));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
