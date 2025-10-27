using System;
using System.Globalization;
using System.Windows.Data;

namespace POMIDOR
{
    // Zwraca klucz grupy: DateTime.Date albo "Bez terminu"
    public sealed class DayKeyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime d) return d.Date;

            if (value is DateTime?)
            {
                var nd = (DateTime?)value;
                return nd.HasValue ? nd.Value.Date : "Bez terminu";
            }

            return "Bez terminu";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    // Ładny nagłówek grupy
    public sealed class GroupHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime d)
                return d.ToString("dddd, dd.MM.yyyy", culture);
            if (value is string s)
                return s;
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
