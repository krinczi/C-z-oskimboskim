using System.Linq; // <- potrzebne dla .Any()
using System.Windows;
using POMIDOR.ViewModels;
using POMIDOR.Models;

namespace POMIDOR.Views;

public partial class GoalsWindow : Window
{
    public GoalsWindow()
    {
        InitializeComponent();
    }

    private MainViewModel VM => (MainViewModel)DataContext;

    private void AddGoal_Click(object sender, RoutedEventArgs e)
    {
        var name = GoalNameBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name)) return;

        // jeśli istnieje, nie dubluj
        if (!VM.Goals.Any(g => string.Equals(g.Name, name, System.StringComparison.OrdinalIgnoreCase)))
        {
            var term = GoalTermBox.SelectedIndex == 1 ? GoalTerm.LongTerm : GoalTerm.ShortTerm;
            VM.Goals.Add(new GoalItem { Name = name, Term = term });
            VM.RecalcAllGoals();
        }

        GoalNameBox.Clear();
        GoalTermBox.SelectedIndex = 0;
    }
}
