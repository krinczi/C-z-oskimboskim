using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace POMIDOR.Models
{
    public class SubTask : INotifyPropertyChanged
    {
        private string _title = "";
        private bool _isCompleted;
        private DateTime? _due;
        private string? _goal;

        public string Title
        {
            get => _title;
            set { if (_title != value) { _title = value; OnPropertyChanged(); } }
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set { if (_isCompleted != value) { _isCompleted = value; OnPropertyChanged(); } }
        }

        public DateTime? Due
        {
            get => _due;
            set { if (_due != value) { _due = value; OnPropertyChanged(); } }
        }

        public string? Goal
        {
            get => _goal;
            set { if (_goal != value) { _goal = value; OnPropertyChanged(); } }
        }

        public SubTask Clone() => new SubTask { Title = Title, IsCompleted = IsCompleted, Due = Due, Goal = Goal };

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
