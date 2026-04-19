using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;
using System.Text.Json;

namespace FreshEstimate.Mobile.Models;

public partial class BusinessDocument : ObservableObject
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [ObservableProperty]
    private int typeValue = (int)DocumentType.Invoice;

    [ObservableProperty]
    private string number = string.Empty;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private DateTime issueDate = DateTime.Today;

    [ObservableProperty]
    private DateTime? dueDate = DateTime.Today.AddDays(14);

    [ObservableProperty]
    private DateTime? validUntil = DateTime.Today.AddDays(14);

    [ObservableProperty]
    private int statusValue = (int)DocumentStatus.Draft;

    [ObservableProperty]
    private string notes = string.Empty;

    public string? CustomerId { get; set; }

    public string? CustomerDisplayName { get; set; }

    public string ItemsJson { get; set; } = "[]";

    [Ignore]
    public Customer? Customer { get; set; }

    [Ignore]
    public List<LineItem> Items { get; set; } = new();

    [Ignore]
    public DocumentType Type
    {
        get => (DocumentType)TypeValue;
        set
        {
            TypeValue = (int)value;
            OnPropertyChanged(nameof(Type));
            OnPropertyChanged(nameof(TypeDisplay));
            OnPropertyChanged(nameof(DateDisplay));
        }
    }

    [Ignore]
    public DocumentStatus Status
    {
        get => (DocumentStatus)StatusValue;
        set
        {
            StatusValue = (int)value;
            OnPropertyChanged(nameof(Status));
        }
    }

    [Ignore]
    public string TypeDisplay => Type == DocumentType.Invoice ? "Invoice" : "Estimate";

    [Ignore]
    public string DateDisplay => Type == DocumentType.Invoice
        ? $"Due: {(DueDate.HasValue ? DueDate.Value.ToShortDateString() : "-")}"
        : $"Valid Until: {(ValidUntil.HasValue ? ValidUntil.Value.ToShortDateString() : "-")}";

    [Ignore]
    public decimal Subtotal => Items.Sum(x => x.Subtotal);

    [Ignore]
    public decimal TaxTotal => Items.Sum(x => x.TaxAmount);

    [Ignore]
    public decimal GrandTotal => Items.Sum(x => x.Total);

    public void SyncPersistenceFields()
    {
        CustomerId = Customer?.Id;
        CustomerDisplayName = Customer?.DisplayName;
        ItemsJson = JsonSerializer.Serialize(Items);
    }

    public void HydrateTransientProperties(IEnumerable<Customer> customers)
    {
        Items = string.IsNullOrWhiteSpace(ItemsJson)
            ? new List<LineItem>()
            : JsonSerializer.Deserialize<List<LineItem>>(ItemsJson) ?? new List<LineItem>();

        Customer = !string.IsNullOrWhiteSpace(CustomerId)
            ? customers.FirstOrDefault(c => c.Id == CustomerId)
            : null;

        if (Customer is null && !string.IsNullOrWhiteSpace(CustomerDisplayName))
        {
            Customer = new Customer
            {
                Id = CustomerId ?? Guid.NewGuid().ToString("N"),
                DisplayName = CustomerDisplayName
            };
        }
    }

    public BusinessDocument Clone()
    {
        return new BusinessDocument
        {
            Id = Id,
            Type = Type,
            Number = Number,
            Title = Title,
            IssueDate = IssueDate,
            DueDate = DueDate,
            ValidUntil = ValidUntil,
            Status = Status,
            Notes = Notes,
            CustomerId = Customer?.Id,
            CustomerDisplayName = Customer?.DisplayName,
            Customer = Customer,
            Items = Items.Select(x => x.Clone()).ToList()
        };
    }
}
