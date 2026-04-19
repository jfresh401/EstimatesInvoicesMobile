using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace FreshEstimate.Mobile.Models;

public partial class LineItemTemplate : ObservableObject
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private decimal defaultQuantity = 1m;

    [ObservableProperty]
    private decimal defaultUnitPrice = 0m;

    [ObservableProperty]
    private decimal defaultTaxRatePercent = 0m;

    public LineItem ToLineItem()
    {
        return new LineItem
        {
            Description = string.IsNullOrWhiteSpace(Description) ? Name : Description,
            Quantity = DefaultQuantity,
            UnitPrice = DefaultUnitPrice,
            TaxRatePercent = DefaultTaxRatePercent
        };
    }
}
