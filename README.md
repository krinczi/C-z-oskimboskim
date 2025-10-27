instalacja nuggeta z konsoli: 
dotnet add POMIDOR/POMIDOR.csproj package OxyPlot.Wpf

==========================================================
Warunki

# Abstrakcja, hermetyzacja, dziedziczenie, polimorfizm

* `Services/Reporting/IReportStrategy.cs`

  * `IReportStrategy` (abstrakcja interfejsowa)
  * `ReportStrategyBase` (klasa abstrakcyjna, dziedziczenie)
  * `DailyReportStrategy`, `WeeklyReportStrategy`, `MonthlyReportStrategy` (polimorfizm przez różne implementacje)
* `Services/AnalyticsService.cs`

  * Przeciążony `Compute(...)` przyjmujący `IReportStrategy` → użycie polimorfizmu zamiast `switch`.
* Hermetyzacja: prywatne pola i metody w wielu klasach (np. `PomodoroWindow.xaml.cs`, `AnalyticsViewModel.cs`, `ReportService.cs`) oraz właściwości tylko-do-odczytu (`Stats` setter prywatny w VM).

# Klasy abstrakcyjne i interfejsy

* `Services/Reporting/IReportStrategy.cs`

  * Interfejs: `IReportStrategy`
  * Klasa abstrakcyjna: `ReportStrategyBase`
* `Services/Storage/IRepository.cs`

  * Interfejs repozytorium: `IRepository<T>`
* Implementacje:

  * `Services/Storage/JsonFileRepository.cs` (implementuje `IRepository<T>`)

# Metody i typy generyczne, delegaty, zdarzenia

* Generyki:

  * `Services/Storage/IRepository.cs` (`IRepository<T>`)
  * `Services/Storage/JsonFileRepository.cs` (`JsonFileRepository<T>`)
  * Cały WPF VM używa `ObservableCollection<T>` (np. `AnalyticsViewModel.cs`, `MainViewModel.cs`)
* Delegaty/zdarzenia:

  * `Services/PomodoroSessionStore.cs` → `public static event EventHandler<PomodoroSessionEventArgs> SessionAppended;`
  * Subskrypcja i reakcja w `ViewModels/AnalyticsViewModel.cs` (auto-recompute po dopisaniu sesji)
  * Dodatkowo WPF-owe `INotifyPropertyChanged` w `AnalyticsViewModel.cs` (zdarzenie `PropertyChanged`)

# Refleksja, atrybuty, serializacja

* Atrybuty:

  * `Models/ReportFieldAttribute.cs` (własny atrybut)
  * Użyty w `Models/Analytics.cs` na właściwościach `StatsResult` (np. `[ReportField("Ukończone zadania", 1)]`)
* Refleksja:

  * `Services/ReportService.cs` → `GetProperties(...)`, `GetCustomAttribute<ReportFieldAttribute>()` do budowy markdown na podstawie atrybutów
* Serializacja:

  * `Services/Storage/JsonFileRepository.cs` (JSON przez `System.Text.Json`)
  * Pośrednio: `Services/PomodoroSessionStore.cs` korzysta z repo do zapisu/odczytu `PomodoroSession`

# Kolekcje (i dlaczego takie)

* `ObservableCollection<T>`: w VM-ach (np. `AnalyticsViewModel.cs`, `MainViewModel.cs`) do live-bindów z UI. WPF tego wymaga, bo emituje zmiany do widoków.
* `List<T>` i `Dictionary<TKey,TValue>`: w `Services/AnalyticsService.cs` do agregacji (grupowania, sumowania, słowniki per dzień). Szybkość i prostota LINQ.
* `IReadOnlyList<T>`: w `IRepository<T>` dla bezpiecznego, niemutowalnego odczytu.

# Wyjątki (własne i przechwytywanie)

* Własny wyjątek: `Models/InvalidDurationException.cs`
* Rzucanie: `Views/PomodoroWindow.xaml.cs` → `EnsureDurations()` zgłasza `InvalidDurationException`
* Obsługa: `Start_Click` i `Reset_Click` w `PomodoroWindow.xaml.cs` łapią wyjątek i pokazują komunikat

