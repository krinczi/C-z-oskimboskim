using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using POMIDOR.Models;

namespace POMIDOR.Views;

public partial class AddTaskDialog : Window
{
    public TodoItem? CreatedItem { get; private set; }
    private TodoItem? _editing;
    private readonly ObservableCollection<SubTask> _subs = new();

    public AddTaskDialog()
    {
        InitializeComponent();
        TitleBox.TextChanged += (_, __) =>
            OkBtn.IsEnabled = !string.IsNullOrWhiteSpace(TitleBox.Text);
        OkBtn.IsEnabled = false;

        SubsList.ItemsSource = _subs;
    }

    public AddTaskDialog(TodoItem toEdit) : this()
    {
        _editing = toEdit;
        Title = "Edytuj zadanie";
        TitleBox.Text = toEdit.Title;
        CategoryBox.Text = toEdit.Category;
        GoalBox.Text = toEdit.Goal;
        DescBox.Text = toEdit.Description;
        DuePicker.SelectedDate = toEdit.Due;
        PriorityBox.SelectedIndex = toEdit.Priority switch
        {
            TaskPriority.Low => 0,
            TaskPriority.Normal => 1,
            TaskPriority.High => 2,
            _ => 1
        };
        foreach (var st in toEdit.SubTasks)
            _subs.Add(st.Clone());

        OkBtn.IsEnabled = true;
    }

    private void AddSub_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SubTitleBox.Text)) return;
        _subs.Add(new SubTask
        {
            Title = SubTitleBox.Text.Trim(),
            Due = SubDuePicker.SelectedDate,
            IsCompleted = false
        });
        SubTitleBox.Clear();
        SubDuePicker.SelectedDate = null;
    }

    private void RemoveSub_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is SubTask st)
            _subs.Remove(st);
    }

    private void OkBtn_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleBox.Text)) return;

        var prioText = (PriorityBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        var priority = prioText switch
        {
            "High" => TaskPriority.High,
            "Low" => TaskPriority.Low,
            _ => TaskPriority.Normal
        };

        CreatedItem = new TodoItem
        {
            Id = _editing?.Id ?? default,
            Title = TitleBox.Text.Trim(),
            Description = string.IsNullOrWhiteSpace(DescBox.Text) ? null : DescBox.Text.Trim(),
            Priority = priority,
            Due = DuePicker.SelectedDate,
            Category = string.IsNullOrWhiteSpace(CategoryBox.Text) ? "Inbox" : CategoryBox.Text.Trim(),
            Goal = string.IsNullOrWhiteSpace(GoalBox.Text) ? null : GoalBox.Text.Trim(),
            IsCompleted = _editing?.IsCompleted ?? false
        };

        // kopiujemy sub-taski do nowego obiektu
        foreach (var st in _subs.Select(s => s.Clone()))
            CreatedItem.SubTasks.Add(st);

        DialogResult = true;
        Close();
    }
}
