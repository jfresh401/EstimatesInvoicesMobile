using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace FreshEstimate.Mobile.Models;

public partial class BrandingSettings : ObservableObject
{
    [PrimaryKey]
    public int Id { get; set; } = 1;

    [ObservableProperty]
    private string businessName = "Fresh Estimate and Invoicing";

    [ObservableProperty]
    private string businessEmail = string.Empty;

    [ObservableProperty]
    private string businessPhone = string.Empty;

    [ObservableProperty]
    private string addressLine1 = string.Empty;

    [ObservableProperty]
    private string addressLine2 = string.Empty;

    [ObservableProperty]
    private string accentHex = "#112033";

    [ObservableProperty]
    private string currencySymbol = "$";

    [ObservableProperty]
    private string paymentInstructions = string.Empty;

    [ObservableProperty]
    private string? logoFilePath;
}
