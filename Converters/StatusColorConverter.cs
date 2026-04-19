using FreshEstimate.Mobile.Models;
using System.Globalization;

namespace FreshEstimate.Mobile.Converters;

public sealed class StatusColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value is DocumentStatus s ? s : DocumentStatus.Draft;

        return status switch
        {
            DocumentStatus.Paid => Color.FromArgb("#DFF8E8"),
            DocumentStatus.Overdue => Color.FromArgb("#FFE3E3"),
            DocumentStatus.Cancelled => Color.FromArgb("#EEF2F7"),
            DocumentStatus.Approved => Color.FromArgb("#E8F0FF"),
            DocumentStatus.Sent or DocumentStatus.Viewed => Color.FromArgb("#FFF4D6"),
            _ => Color.FromArgb("#F3F4F6")
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
