using System.Globalization;
using FreshEstimate.Mobile.Models;

namespace FreshEstimate.Mobile.Converters;

public sealed class EstimateToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is DocumentType type && type == DocumentType.Estimate;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}