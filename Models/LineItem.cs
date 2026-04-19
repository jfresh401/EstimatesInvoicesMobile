using CommunityToolkit.Mvvm.ComponentModel;

namespace FreshEstimate.Mobile.Models;

public partial class LineItem : ObservableObject
{
    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private decimal quantity = 1m;

    [ObservableProperty]
    private decimal unitPrice = 0m;

    [ObservableProperty]
    private decimal taxRatePercent = 0m;

    public decimal Subtotal => Quantity * UnitPrice;
    public decimal TaxAmount => Math.Round(Subtotal * (TaxRatePercent / 100m), 2, MidpointRounding.AwayFromZero);
    public decimal Total => Subtotal + TaxAmount;

    public LineItem Clone()
    {
        return new LineItem
        {
            Description = Description,
            Quantity = Quantity,
            UnitPrice = UnitPrice,
            TaxRatePercent = TaxRatePercent
        };
    }
}
