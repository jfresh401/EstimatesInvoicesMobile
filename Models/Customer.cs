using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace FreshEstimate.Mobile.Models;

public partial class Customer : ObservableObject
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [ObservableProperty]
    private string displayName = string.Empty;

    [ObservableProperty]
    private string contactName = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string phone = string.Empty;

    [ObservableProperty]
    private string billingAddress = string.Empty;

    [ObservableProperty]
    private string notes = string.Empty;

    [Ignore]
    public string FullDetails => string.Join(Environment.NewLine, new[]
    {
        DisplayName,
        ContactName,
        Email,
        Phone,
        BillingAddress
    }.Where(x => !string.IsNullOrWhiteSpace(x)));
}
