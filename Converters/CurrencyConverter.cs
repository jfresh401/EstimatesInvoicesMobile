using System.Globalization;

namespace FreshEstimate.Mobile.Converters;

public sealed class CurrencyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is decimal number ? number.ToString("C2", culture) : "$0.00";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
