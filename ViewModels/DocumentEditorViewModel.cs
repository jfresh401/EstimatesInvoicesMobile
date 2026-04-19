using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreshEstimate.Mobile.Models;
using FreshEstimate.Mobile.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FreshEstimate.Mobile.ViewModels;

public partial class DocumentEditorViewModel : ObservableObject
{
    private readonly AppRepository _repository;
    private BusinessDocument? _subscribedDocument;

    public ObservableCollection<Customer> Customers { get; } = new();
    public ObservableCollection<LineItemTemplate> Templates { get; } = new();
    public ObservableCollection<LineItem> Items { get; } = new();

    [ObservableProperty] private BusinessDocument workingDocument = new();
    [ObservableProperty] private Customer? selectedCustomer;
    [ObservableProperty] private LineItemTemplate? selectedTemplate;

    public Array Statuses => Enum.GetValues(typeof(DocumentStatus));
    public Array Types => Enum.GetValues(typeof(DocumentType));

    public string PageTitle =>
        string.IsNullOrWhiteSpace(WorkingDocument.Number) ? "Document" : WorkingDocument.Number;

    public decimal GrandTotal => Items.Sum(x => x.Total);

    public string SecondaryDateLabel =>
        WorkingDocument.Type == DocumentType.Invoice ? "Due Date" : "Valid Until";

    public DateTime SecondaryDate
    {
        get => WorkingDocument.Type == DocumentType.Invoice
            ? WorkingDocument.DueDate ?? DateTime.Today.AddDays(14)
            : WorkingDocument.ValidUntil ?? DateTime.Today.AddDays(14);
        set
        {
            if (WorkingDocument.Type == DocumentType.Invoice)
                WorkingDocument.DueDate = value;
            else
                WorkingDocument.ValidUntil = value;

            OnPropertyChanged(nameof(SecondaryDate));
        }
    }

    public DocumentEditorViewModel(AppRepository repository)
    {
        _repository = repository;
    }

    public async Task LoadAsync(BusinessDocument? document, DocumentType? defaultType = null)
    {
        UnsubscribeDocument();
        ClearItems();

        var customers = await _repository.GetCustomersAsync();
        var templates = await _repository.GetTemplatesAsync();

        Customers.Clear();
        Templates.Clear();

        foreach (var customer in customers)
            Customers.Add(customer);

        foreach (var template in templates)
            Templates.Add(template);

        WorkingDocument = document?.Clone() ?? CreateDefault(defaultType ?? DocumentType.Invoice);

        if (WorkingDocument.Items == null)
            WorkingDocument.Items = new List<LineItem>();

        if (WorkingDocument.Items.Count == 0)
        {
            WorkingDocument.Items.Add(new LineItem
            {
                Description = "Service",
                Quantity = 1m,
                UnitPrice = 0m,
                TaxRatePercent = 0m
            });
        }

        _subscribedDocument = WorkingDocument;
        _subscribedDocument.PropertyChanged += WorkingDocument_PropertyChanged;

        SelectedCustomer = Customers.FirstOrDefault(c => c.Id == WorkingDocument.Customer?.Id)
            ?? Customers.FirstOrDefault(c => c.Id == WorkingDocument.CustomerId)
            ?? Customers.FirstOrDefault(c => c.DisplayName == WorkingDocument.CustomerDisplayName)
            ?? WorkingDocument.Customer;

        foreach (var item in WorkingDocument.Items)
        {
            item.PropertyChanged += OnItemChanged;
            Items.Add(item);
        }

        SelectedTemplate = null;

        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(GrandTotal));
        OnPropertyChanged(nameof(SecondaryDate));
        OnPropertyChanged(nameof(SecondaryDateLabel));
    }

    private void WorkingDocument_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(BusinessDocument.Type) or nameof(BusinessDocument.DueDate) or nameof(BusinessDocument.ValidUntil))
        {
            OnPropertyChanged(nameof(SecondaryDate));
            OnPropertyChanged(nameof(SecondaryDateLabel));
        }

        if (e.PropertyName == nameof(BusinessDocument.Number))
            OnPropertyChanged(nameof(PageTitle));
    }

    private static BusinessDocument CreateDefault(DocumentType type)
    {
        var countSeed = DateTime.Now.ToString("yyyyMMddHHmmss");

        return new BusinessDocument
        {
            Type = type,
            Number = type == DocumentType.Invoice ? $"INV-{countSeed}" : $"EST-{countSeed}",
            Title = type == DocumentType.Invoice ? "New Invoice" : "New Estimate",
            IssueDate = DateTime.Today,
            DueDate = type == DocumentType.Invoice ? DateTime.Today.AddDays(14) : null,
            ValidUntil = type == DocumentType.Estimate ? DateTime.Today.AddDays(14) : null,
            Status = DocumentStatus.Draft,
            Items = new List<LineItem>()
        };
    }

    private void OnItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(GrandTotal));
    }

    private void ClearItems()
    {
        foreach (var item in Items)
            item.PropertyChanged -= OnItemChanged;

        Items.Clear();
    }

    private void UnsubscribeDocument()
    {
        if (_subscribedDocument is not null)
            _subscribedDocument.PropertyChanged -= WorkingDocument_PropertyChanged;

        _subscribedDocument = null;
    }

    [RelayCommand]
    private void AddBlankLine()
    {
        var item = new LineItem
        {
            Description = "Service",
            Quantity = 1m,
            UnitPrice = 0m,
            TaxRatePercent = 0m
        };

        item.PropertyChanged += OnItemChanged;
        Items.Add(item);
        OnPropertyChanged(nameof(GrandTotal));
    }

    [RelayCommand]
    private void AddTemplateLine()
    {
        if (SelectedTemplate is null)
            return;

        var item = SelectedTemplate.ToLineItem();
        item.PropertyChanged += OnItemChanged;
        Items.Add(item);
        OnPropertyChanged(nameof(GrandTotal));
    }

    [RelayCommand]
    private void RemoveLine(LineItem item)
    {
        if (item is null)
            return;

        item.PropertyChanged -= OnItemChanged;
        Items.Remove(item);
        OnPropertyChanged(nameof(GrandTotal));
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(WorkingDocument.Title))
        {
            await Shell.Current.DisplayAlert("Missing title", "Document title is required.", "OK");
            return;
        }

        if (SelectedCustomer is null)
        {
            await Shell.Current.DisplayAlert("Missing customer", "Choose a customer before saving.", "OK");
            return;
        }

        if (Items.Count == 0)
        {
            await Shell.Current.DisplayAlert("Missing line items", "Add at least one line item before saving.", "OK");
            return;
        }

        WorkingDocument.Customer = SelectedCustomer;
        WorkingDocument.CustomerId = SelectedCustomer.Id;
        WorkingDocument.CustomerDisplayName = SelectedCustomer.DisplayName;
        WorkingDocument.Items = Items.Select(x => x.Clone()).ToList();
        WorkingDocument.SyncPersistenceFields();

        await _repository.SaveDocumentAsync(WorkingDocument);
        await Shell.Current.Navigation.PopModalAsync();
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }
}