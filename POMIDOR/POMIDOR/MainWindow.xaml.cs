using POMIDOR.Models;
using POMIDOR.ViewModels;
using POMIDOR.Views;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace POMIDOR;

public partial class MainWindow : Window
{
    private enum ViewRange { Day, Week, Month }

    private ViewRange _range = ViewRange.Week;
    private DateTime _anchor = DateTime.Today;
    private DateTime _from, _to; // [from, to)

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();

        // Komendy dla menu kontekstowego
        CommandBindings.Add(new CommandBinding(AppCommands.Edit, (s, e) => { if (e.Parameter is TodoItem it) EditItem(it); }, CanExec));
        CommandBindings.Add(new CommandBinding(AppCommands.Delete, (s, e) => { if (e.Parameter is TodoItem it) DeleteItem(it); }, CanExec));
        CommandBindings.Add(new CommandBinding(AppCommands.MarkDone, (s, e) => { if (e.Parameter is TodoItem it) it.IsCompleted = true; }, CanExec));
        CommandBindings.Add(new CommandBinding(AppCommands.MarkUndone, (s, e) => { if (e.Parameter is TodoItem it) it.IsCompleted = false; }, CanExec));
    }

    private MainViewModel VM => (MainViewModel)DataContext;
    private ICollectionView View => CollectionViewSource.GetDefaultView(ItemsList.ItemsSource);

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _range = RangeBox.SelectedIndex switch
        {
            0 => ViewRange.Day,
            1 => ViewRange.Week,
            2 => ViewRange.Month,
            _ => ViewRange.Week
        };

        RecalcPeriod();

        // Filtr pod zakres dnia/tygodnia/miesiąca
        View.Filter = TasksFilter;

        // Sort: niewykonane na górze, potem po terminie
        using (View.DeferRefresh())
        {
            View.SortDescriptions.Clear();
            View.SortDescriptions.Add(new SortDescription(nameof(TodoItem.IsCompleted), ListSortDirection.Ascending));
            View.SortDescriptions.Add(new SortDescription(nameof(TodoItem.Due), ListSortDirection.Ascending));
        }

        ApplyGrouping();
        UpdatePeriodLabel();
    }

    private bool TasksFilter(object obj)
    {
        if (obj is not TodoItem t) return false;
        if (!t.Due.HasValue) return false; // bez terminu nie trafia do widoków czasu
        var d = t.Due.Value.Date;
        return d >= _from && d < _to;
    }

    private void RecalcPeriod()
    {
        switch (_range)
        {
            case ViewRange.Day:
                _from = _anchor.Date;
                _to = _from.AddDays(1);
                break;
            case ViewRange.Week:
                // tydzień od poniedziałku
                int delta = ((int)_anchor.DayOfWeek - 1 + 7) % 7;
                _from = _anchor.Date.AddDays(-delta);
                _to = _from.AddDays(7);
                break;
            case ViewRange.Month:
                _from = new DateTime(_anchor.Year, _anchor.Month, 1);
                _to = _from.AddMonths(1);
                break;
        }
    }

    private void ApplyGrouping()
    {
        var dayConv = (IValueConverter)FindResource("DayKeyConv");

        using (View.DeferRefresh())
        {
            View.GroupDescriptions.Clear();

            // Zawsze grupuj po dniu (dla aktywnego zakresu czasu)
            View.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TodoItem.Due), dayConv));

            // Dodatkowe grupowanie (opcjonalnie)
            var selected = (GroupByBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (selected == "Kategoria")
                View.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TodoItem.Category)));
            else if (selected == "Cel")
                View.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TodoItem.Goal)));
        }
    }

    private void UpdatePeriodLabel()
    {
        var culture = CultureInfo.CurrentCulture;
        string txt = _range switch
        {
            ViewRange.Day => _from.ToString("dddd, dd.MM.yyyy", culture),
            ViewRange.Week => $"{_from:dd.MM.yyyy} – {_to.AddDays(-1):dd.MM.yyyy}",
            ViewRange.Month => _from.ToString("MMMM yyyy", culture),
            _ => ""
        };
        PeriodLabel.Text = txt;
    }

    private void CanExec(object? s, CanExecuteRoutedEventArgs e)
        => e.CanExecute = e.Parameter is TodoItem;

    // Dodawanie
    private void AddTask_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new Views.AddTaskDialog { Owner = this };
        if (dlg.ShowDialog() == true && dlg.CreatedItem is not null)
        {
            if (dlg.CreatedItem.Id == default) dlg.CreatedItem.Id = Guid.NewGuid();
            VM.Items.Add(dlg.CreatedItem);
            View.Refresh(); // żeby nowy element wpadł w bieżący okres
        }
    }

    // Double-click = edycja
    private void Item_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ItemsList.SelectedItem is TodoItem item)
            EditItem(item);
    }

    // Przyciski w kolumnie "Akcje"
    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is TodoItem item)
            EditItem(item);
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is TodoItem item)
            DeleteItem(item);
    }

    private void EditItem(TodoItem item)
    {
        var dlg = new Views.AddTaskDialog(item) { Owner = this };
        if (dlg.ShowDialog() == true && dlg.CreatedItem is not null)
        {
            var src = dlg.CreatedItem;
            item.Title = src.Title;
            item.Description = src.Description;
            item.Priority = src.Priority;
            item.Due = src.Due;
            item.Category = src.Category;
            item.Goal = src.Goal;
            View.Refresh();
        }
    }

    private void DeleteItem(TodoItem item)
    {
        var confirm = MessageBox.Show(
            $"Usunąć zadanie „{item.Title}”?",
            "Potwierdź usunięcie",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm == MessageBoxResult.Yes)
        {
            VM.Items.Remove(item);
            View.Refresh();
        }
    }

    // Zmiana grupowania dodatkowego
    private void GroupByBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        ApplyGrouping();
    }

    // Zmiana zakresu (dzień/tydzień/miesiąc)
    private void RangeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        _range = RangeBox.SelectedIndex switch
        {
            0 => ViewRange.Day,
            1 => ViewRange.Week,
            2 => ViewRange.Month,
            _ => _range
        };
        RecalcPeriod();
        View.Refresh();
        ApplyGrouping();
        UpdatePeriodLabel();
    }

    // Nawigacja datą
    private void Prev_Click(object sender, RoutedEventArgs e)
    {
        _anchor = _range switch
        {
            ViewRange.Day => _anchor.AddDays(-1),
            ViewRange.Week => _anchor.AddDays(-7),
            ViewRange.Month => _anchor.AddMonths(-1),
            _ => _anchor
        };
        RecalcPeriod(); View.Refresh(); UpdatePeriodLabel();
    }

    private void Next_Click(object sender, RoutedEventArgs e)
    {
        _anchor = _range switch
        {
            ViewRange.Day => _anchor.AddDays(1),
            ViewRange.Week => _anchor.AddDays(7),
            ViewRange.Month => _anchor.AddMonths(1),
            _ => _anchor
        };
        RecalcPeriod(); View.Refresh(); UpdatePeriodLabel();
    }

    private void Today_Click(object sender, RoutedEventArgs e)
    {
        _anchor = DateTime.Today;
        RecalcPeriod(); View.Refresh(); UpdatePeriodLabel();
    }

    private void OpenGoals_Click(object sender, RoutedEventArgs e)
    {
        // odśwież wyliczenia przed pokazaniem okna
        VM.RecalcAllGoals();

        var win = new GoalsWindow
        {
            Owner = this,
            DataContext = VM
        };
        win.ShowDialog();
    }

    private void OpenPomodoro_Click(object sender, RoutedEventArgs e)
    {
        new PomodoroWindow { Owner = this }.Show();
    }

    // >>> TU NOWE: otwarcie okna Analizy
    private void OpenAnalytics_Click(object sender, RoutedEventArgs e)
    {
        var mainVm = VM; // te same kolekcje co w MainViewModel
        var vm = new AnalyticsViewModel(mainVm.Items, mainVm.Goals);

        var win = new AnalyticsWindow(vm)
        {
            Owner = this
        };
        win.ShowDialog();
    }
    // <<< KONIEC NOWEGO

    // Rozwijanie panelu sub-tasków z wiersza
    private void ShowSubs_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is SubTask st &&
            ItemsList.SelectedItem is TodoItem parent)
        {
            parent.SubTasks.Remove(st);
        }
    }

    // Usuwanie sub-tasków z panelu
    private void RemoveSubFromPanel_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is SubTask st &&
            ItemsList.SelectedItem is TodoItem parent)
        {
            parent.SubTasks.Remove(st);
        }
    }
}
