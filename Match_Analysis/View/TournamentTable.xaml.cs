using System;
using System.Globalization;
using System.Windows.Data;

public class ReleaseDateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || (value is DateTime dt && dt == DateTime.MinValue))
        {
            return "В команде по настоящее время";
        }
        return ((DateTime)value).ToShortDateString(); // или .ToString("dd.MM.yyyy")
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}