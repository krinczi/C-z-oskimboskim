using System;
using System.Media;
using System.Windows;
using System.Windows.Threading;
using POMIDOR.Models;
using POMIDOR.Services;

namespace POMIDOR.Views
{
    public partial class PomodoroWindow : Window
    {
        private enum PomodoroPhase { Idle, Work, Break }

        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        private PomodoroPhase _phase = PomodoroPhase.Idle;
        private TimeSpan _remaining;
        private TimeSpan _workDur = TimeSpan.FromMinutes(25);
        private TimeSpan _breakDur = TimeSpan.FromMinutes(5);

        private DateTime? _phaseStartUtc = null;

        private int _completedCycles = 0;
        private long _totalWorkSeconds = 0;
        private long _totalBreakSeconds = 0;

        public PomodoroWindow()
        {
            InitializeComponent();
            _timer.Tick += Timer_Tick;
            SetPhase(PomodoroPhase.Idle, resetTime: true);
            UpdateStatsUi();
        }

        private void EnsureDurations()
        {
            if (!int.TryParse(WorkBox.Text, out var w) || w < 1 || w > 120)
                throw new InvalidDurationException("Nieprawidłowa długość pracy (1–120 min).");
            if (!int.TryParse(BreakBox.Text, out var b) || b < 1 || b > 60)
                throw new InvalidDurationException("Nieprawidłowa długość przerwy (1–60 min).");
            _workDur = TimeSpan.FromMinutes(w);
            _breakDur = TimeSpan.FromMinutes(b);
        }

        private void SetPhase(PomodoroPhase ph, bool resetTime)
        {
            _phase = ph;

            if (resetTime)
            {
                _remaining = ph switch
                {
                    PomodoroPhase.Work => _workDur,
                    PomodoroPhase.Break => _breakDur,
                    _ => TimeSpan.Zero
                };
            }

            PhaseLabel.Text = ph switch
            {
                PomodoroPhase.Work => "Tryb: praca",
                PomodoroPhase.Break => "Tryb: przerwa",
                _ => "Tryb: bezczynny"
            };

            _phaseStartUtc = (ph == PomodoroPhase.Work || ph == PomodoroPhase.Break) ? DateTime.UtcNow : null;

            UpdateUi();
        }

        private void UpdateUi()
        {
            TimerLabel.Text = $"{(int)_remaining.TotalMinutes:00}:{_remaining.Seconds:00}";

            if (StartBtn != null && PauseBtn != null && ResetBtn != null)
            {
                StartBtn.IsEnabled = _phase == PomodoroPhase.Idle || _phase == PomodoroPhase.Break;
                PauseBtn.IsEnabled = _phase == PomodoroPhase.Work || _phase == PomodoroPhase.Break;
                ResetBtn.IsEnabled = true;
            }
        }

        private void UpdateStatsUi()
        {
            CyclesText.Text = _completedCycles.ToString();
            TimeSpan ws = TimeSpan.FromSeconds(_totalWorkSeconds);
            TimeSpan bs = TimeSpan.FromSeconds(_totalBreakSeconds);
            TotalWorkText.Text = $"{(int)ws.TotalHours:00}:{ws.Minutes:00}:{ws.Seconds:00}";
            TotalBreakText.Text = $"{(int)bs.TotalHours:00}:{bs.Minutes:00}:{bs.Seconds:00}";
        }

        private void FinishWorkPhase()
        {
            _completedCycles += 1;
            _totalWorkSeconds += (long)_workDur.TotalSeconds;

            if (_phaseStartUtc.HasValue)
            {
                var sess = new PomodoroSession
                {
                    StartUtc = _phaseStartUtc.Value,
                    EndUtc = DateTime.UtcNow
                };
                PomodoroSessionStore.Append(sess);
            }

            var stats = PomodoroStore_LoadOrDefault();
            stats.CompletedCycles = _completedCycles;
            stats.TotalWorkSeconds = _totalWorkSeconds;
            stats.TotalBreakSeconds = _totalBreakSeconds;
            stats.LastUpdated = DateTime.UtcNow;
            PomodoroStore_Save(stats);

            if (SoundBox.IsChecked == true) SystemSounds.Asterisk.Play();
            UpdateStatsUi();
        }

        private void FinishBreakPhase()
        {
            _totalBreakSeconds += (long)_breakDur.TotalSeconds;
            if (SoundBox.IsChecked == true) SystemSounds.Beep.Play();
            UpdateStatsUi();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_remaining > TimeSpan.Zero)
            {
                _remaining -= TimeSpan.FromSeconds(1);
                UpdateUi();
                return;
            }

            _timer.Stop();

            if (_phase == PomodoroPhase.Work)
            {
                FinishWorkPhase();

                if (AutoSwitchBox.IsChecked == true)
                {
                    SetPhase(PomodoroPhase.Break, resetTime: true);
                    _timer.Start();
                }
                else
                {
                    SetPhase(PomodoroPhase.Idle, resetTime: true);
                }
            }
            else if (_phase == PomodoroPhase.Break)
            {
                FinishBreakPhase();

                if (AutoSwitchBox.IsChecked == true)
                {
                    SetPhase(PomodoroPhase.Work, resetTime: true);
                    _timer.Start();
                }
                else
                {
                    SetPhase(PomodoroPhase.Idle, resetTime: true);
                }
            }
        }

        // placeholder na wypadek, gdy nie masz swojego magazynu
        private PomodoroStats PomodoroStore_LoadOrDefault()
            => new PomodoroStats
            {
                CompletedCycles = _completedCycles,
                TotalWorkSeconds = _totalWorkSeconds,
                TotalBreakSeconds = _totalBreakSeconds,
                LastUpdated = DateTime.UtcNow
            };

        private void PomodoroStore_Save(PomodoroStats s) { /* no-op */ }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EnsureDurations();
                if (_phase == PomodoroPhase.Idle) SetPhase(PomodoroPhase.Work, resetTime: true);
                _timer.Start();
            }
            catch (InvalidDurationException ex)
            {
                MessageBox.Show(this, ex.Message, "Błędne czasy", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Pause_Click(object sender, RoutedEventArgs e) => _timer.Stop();

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            try
            {
                EnsureDurations();
                SetPhase(PomodoroPhase.Idle, resetTime: true);
            }
            catch (InvalidDurationException ex)
            {
                MessageBox.Show(this, ex.Message, "Błędne czasy", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // minimalny typ na potrzeby zgodności z istniejącymi wywołaniami
        private sealed class PomodoroStats
        {
            public int CompletedCycles;
            public long TotalWorkSeconds;
            public long TotalBreakSeconds;
            public DateTime LastUpdated;
        }
    }
}
