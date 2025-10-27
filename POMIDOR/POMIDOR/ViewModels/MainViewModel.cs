using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using POMIDOR.Models;

namespace POMIDOR.ViewModels;

public class MainViewModel
{
    public ObservableCollection<TodoItem> Items { get; } = new();
    public ObservableCollection<GoalItem> Goals { get; } = new();

    public MainViewModel()
    {
        // reaguj na zmiany listy zadań
        Items.CollectionChanged += Items_CollectionChanged;
    }

    void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (var it in e.NewItems.OfType<TodoItem>())
            {
                it.PropertyChanged += Todo_PropertyChanged;
                it.SubTasks.CollectionChanged += Subs_CollectionChanged;
                foreach (var s in it.SubTasks) s.PropertyChanged += Sub_PropertyChanged;
                EnsureGoalExists(it.Goal);
                RecalcAllGoals();
            }
        }
        if (e.OldItems != null)
        {
            foreach (var it in e.OldItems.OfType<TodoItem>())
            {
                it.PropertyChanged -= Todo_PropertyChanged;
                it.SubTasks.CollectionChanged -= Subs_CollectionChanged;
                foreach (var s in it.SubTasks) s.PropertyChanged -= Sub_PropertyChanged;
                RecalcAllGoals();
            }
        }
    }

    void Subs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
            foreach (var s in e.NewItems.OfType<SubTask>()) s.PropertyChanged += Sub_PropertyChanged;
        if (e.OldItems != null)
            foreach (var s in e.OldItems.OfType<SubTask>()) s.PropertyChanged -= Sub_PropertyChanged;

        // ktoś dodał/usunął sub-task → przelicz
        RecalcAllGoals();
    }

    void Todo_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(TodoItem.IsCompleted) or nameof(TodoItem.Goal) or nameof(TodoItem.ProgressPercent))
        {
            if (sender is TodoItem t) EnsureGoalExists(t.Goal);
            RecalcAllGoals();
        }
    }

    void Sub_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(SubTask.IsCompleted) or nameof(SubTask.Goal))
        {
            if (sender is SubTask s) EnsureGoalExists(s.Goal);
            RecalcAllGoals();
        }
    }

    public void EnsureGoalExists(string? goalName)
    {
        var name = string.IsNullOrWhiteSpace(goalName) ? null : goalName.Trim();
        if (string.IsNullOrEmpty(name)) return;
        if (!Goals.Any(g => string.Equals(g.Name, name, System.StringComparison.OrdinalIgnoreCase)))
        {
            Goals.Add(new GoalItem { Name = name, Term = GoalTerm.ShortTerm });
        }
    }

    public void RecalcAllGoals()
    {
        foreach (var g in Goals)
            g.Recalculate(Items);
    }
}
