using System.Windows.Input;

namespace POMIDOR;

public static class AppCommands
{
    public static readonly RoutedUICommand Edit = new("Edit", nameof(Edit), typeof(AppCommands));
    public static readonly RoutedUICommand Delete = new("Delete", nameof(Delete), typeof(AppCommands));
    public static readonly RoutedUICommand MarkDone = new("MarkDone", nameof(MarkDone), typeof(AppCommands));
    public static readonly RoutedUICommand MarkUndone = new("MarkUndone", nameof(MarkUndone), typeof(AppCommands));
}
