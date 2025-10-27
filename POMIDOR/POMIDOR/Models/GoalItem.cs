using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Collections.Generic;

namespace POMIDOR.Models;

public class GoalItem : INotifyPropertyChanged
{
    string _name = "";
    GoalTerm _term = GoalTerm.ShortTerm;

    int _progressPercent;
    int _tasksCount;
    int _tasksDone;          // pełne zadania ukończone (100%)
    int _subsAssigned;       // suby przypisane bezpośrednio do celu (gdy rodzic nie ma tego celu)
    int _subsAssignedDone;

    public string Name
    {
        get => _name;
        set { if (_name != value) { _name = value; OnPropertyChanged(); } }
    }

    public GoalTerm Term
    {
        get => _term;
        set { if (_term != value) { _term = value; OnPropertyChanged(); } }
    }

    public int ProgressPercent
    {
        get => _progressPercent;
        private set { if (_progressPercent != value) { _progressPercent = value; OnPropertyChanged(); } }
    }

    public int TasksCount
    {
        get => _tasksCount;
        private set { if (_tasksCount != value) { _tasksCount = value; OnPropertyChanged(); } }
    }

    public int TasksDone
    {
        get => _tasksDone;
        private set { if (_tasksDone != value) { _tasksDone = value; OnPropertyChanged(); } }
    }

    public int SubsAssigned
    {
        get => _subsAssigned;
        private set { if (_subsAssigned != value) { _subsAssigned = value; OnPropertyChanged(); } }
    }

    public int SubsAssignedDone
    {
        get => _subsAssignedDone;
        private set { if (_subsAssignedDone != value) { _subsAssignedDone = value; OnPropertyChanged(); } }
    }

    /// <summary>
    /// Liczy progres celu na podstawie zadań i sub-tasków.
    /// Zasada:
    /// - Jeśli zadanie ma Goal == this.Name, liczymy je jako 1 „wagę” i bierzemy jego ProgressPercent.
    /// - Jeśli rodzic NIE ma tego celu, ale któreś suby mają Goal == this.Name, liczymy je pojedynczo (0/1).
    /// Unikamy podwójnego liczenia.
    /// </summary>
    public void Recalculate(IEnumerable<TodoItem> allItems)
    {
        if (allItems is null) return;

        double completedWeight = 0;
        double totalWeight = 0;

        int tasksCount = 0;
        int tasksDone = 0;
        int subsAssign = 0;
        int subsAssignDone = 0;

        foreach (var t in allItems)
        {
            bool taskAssigned = string.Equals(t.Goal, Name, StringComparison.OrdinalIgnoreCase);
            if (taskAssigned)
            {
                // zadanie przypisane do celu: waga 1, wkład = ProgressPercent/100
                totalWeight += 1;
                completedWeight += t.ProgressPercent / 100.0;

                tasksCount++;
                if (t.ProgressPercent >= 100) tasksDone++;
            }
            else
            {
                // nie przypisane zadanie; sprawdź suby przypisane bezpośrednio do celu
                foreach (var s in t.SubTasks)
                {
                    if (string.Equals(s.Goal, Name, StringComparison.OrdinalIgnoreCase))
                    {
                        totalWeight += 1;
                        completedWeight += s.IsCompleted ? 1 : 0;

                        subsAssign++;
                        if (s.IsCompleted) subsAssignDone++;
                    }
                }
            }
        }

        TasksCount = tasksCount;
        TasksDone = tasksDone;
        SubsAssigned = subsAssign;
        SubsAssignedDone = subsAssignDone;

        int percent = totalWeight <= 0 ? 0 : (int)Math.Round(100.0 * completedWeight / totalWeight);
        ProgressPercent = Math.Clamp(percent, 0, 100);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
