using System.Windows;
using POMIDOR.ViewModels;

namespace POMIDOR.Views
{
    public partial class AnalyticsWindow : Window
    {
        public AnalyticsViewModel Vm { get; }

        public AnalyticsWindow(AnalyticsViewModel vm)
        {
            InitializeComponent();
            Vm = vm;
            DataContext = Vm;
        }

        private void Day_Click(object sender, RoutedEventArgs e) => Vm.Compute(Models.ReportKind.Daily);
        private void Week_Click(object sender, RoutedEventArgs e) => Vm.Compute(Models.ReportKind.Weekly);
        private void Month_Click(object sender, RoutedEventArgs e) => Vm.Compute(Models.ReportKind.Monthly);

        private void Recompute_Click(object sender, RoutedEventArgs e)
        {
            // Domyślnie liczmy tydzień, bo najbardziej informacyjny
            Vm.Compute(Models.ReportKind.Weekly);
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var (md, csv) = Vm.GenerateReportFiles();
            MessageBox.Show(this, $"Zapisano:\n• {md}\n• {csv}", "Raport wygenerowany", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
