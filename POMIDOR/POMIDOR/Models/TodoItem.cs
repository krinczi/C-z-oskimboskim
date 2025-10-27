using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace POMIDOR.Models;

public class TodoItem : INotifyPropertyChanged
{
    private Guid _id = Guid.NewGuid();
    private string _title = "";
    private string? _description;
    private TaskPriority _priority = TaskPriority.Normal;
    private DateTime? _due;
    private string _category = "Inbox";
    private string? _goal;
    private bool _isCompleted;
    private bool _suppressCascade; // anty-pętla przy synchronizacji z sub-taskami

    public TodoItem()
    {
        SubTasks.CollectionChanged += SubTasks_CollectionChanged;
    }

    public Guid Id
    {
        get => _id;
        set { if (_id != value) { _id = value; OnPropertyChanged(); } }
    }

    public string Title
    {
        get => _title;
        set { if (_title != value) { _title = value; OnPropertyChanged(); } }
    }

    public string? Description
    {
        get => _description;
        set { if (_description != value) { _description = value; OnPropertyChanged(); } }
    }

    public TaskPriority Priority
    {
        get => _priority;
        set { if (_priority != value) { _priority = value; OnPropertyChanged(); } }
    }

    public DateTime? Due
    {
        get => _due;
        set { if (_due != value) { _due = value; OnPropertyChanged(); } }
    }

    public string Category
    {
        get => _category;
        set { if (_category != value) { _category = value; OnPropertyChanged(); } }
    }

    public string? Goal
    {
        get => _goal;
        set { if (_goal != value) { _goal = value; OnPropertyChanged(); } }
    }

    public bool IsCompleted
    {
        get => _isCompleted;
        set
        {
            if (_isCompleted == value) return;
            _isCompleted = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ProgressPercent));

            // kaskada: zmiana na zadaniu głównym ustawia wszystkie suby
            if (!_suppressCascade)
            {
                _suppressCascade = true;
                foreach (var st in SubTasks)
                    st.IsCompleted = value;
                _suppressCascade = false;
            }
        }
    }

    // --- SUBTASKI ---
    public ObservableCollection<SubTask> SubTasks { get; } = new();

    public int SubTasksTotal => SubTasks.Count;
    public int SubTasksDone => SubTasks.Count(s => s.IsCompleted);

    // 0 sub-tasków = 0% chyba że task ręcznie oznaczony ukończony → 100%
    public int ProgressPercent
        => SubTasksTotal == 0 ? (_isCompleted ? 100 : 0)
                              : (int)Math.Round(SubTasksDone * 100.0 / SubTasksTotal);

    private void SubTasks_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
            foreach (var it in e.OldItems.OfType<SubTask>())
                it.PropertyChanged -= SubTask_PropertyChanged;

        if (e.NewItems != null)
            foreach (var it in e.NewItems.OfType<SubTask>())
                it.PropertyChanged += SubTask_PropertyChanged;

        OnPropertyChanged(nameof(SubTasksTotal));
        OnPropertyChanged(nameof(SubTasksDone));
        OnPropertyChanged(nameof(ProgressPercent));
        SyncParentCompletionFromSubs();
    }

    private void SubTask_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SubTask.IsCompleted) ||
            e.PropertyName == nameof(SubTask.Title) ||
            e.PropertyName == nameof(SubTask.Due))
        {
            OnPropertyChanged(nameof(SubTasksTotal));
            OnPropertyChanged(nameof(SubTasksDone));
            OnPropertyChanged(nameof(ProgressPercent));
            if (e.PropertyName == nameof(SubTask.IsCompleted))
                SyncParentCompletionFromSubs();
        }
    }

    // rodzic = ukończony tylko jeśli są suby i wszystkie są ukończone
    private void SyncParentCompletionFromSubs()
    {
        if (_suppressCascade) return;

        bool allDone = SubTasks.Count > 0 && SubTasks.All(s => s.IsCompleted);

        if (_isCompleted != allDone)
        {
            _suppressCascade = true;
            _isCompleted = allDone;
            OnPropertyChanged(nameof(IsCompleted));
            _suppressCascade = false;
        }
        OnPropertyChanged(nameof(ProgressPercent));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
