using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreshEstimate.Mobile.Models;
using FreshEstimate.Mobile.Services;
using System.Collections.ObjectModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace FreshEstimate.Mobile.ViewModels;

public partial class DocumentsViewModel : ObservableObject
{
    private readonly AppRepository _repository;
    private readonly PdfExportService _pdfExportService;

    public ObservableCollection<BusinessDocument> Documents { get; } = new();

    public ObservableCollection<string> SortOptions { get; } = new(new[]
    {
        "Newest",
        "Oldest",
        "Customer A-Z",
        "Total High-Low",
        "Total Low-High"
    });

    public ObservableCollection<string> FilterOptions { get; } = new(new[]
    {
        "All",
        "Invoices",
        "Estimates",
        "Draft",
        "Sent",
        "Paid",
        "Overdue",
        "Cancelled"
    });

    [ObservableProperty] private string selectedSort = "Newest";
    [ObservableProperty] private string selectedFilter = "All";
    [ObservableProperty] private string searchText = string.Empty;

    public DocumentsViewModel(AppRepository repository, PdfExportService pdfExportService)
    {
        _repository = repository;
        _pdfExportService = pdfExportService;
        _repository.DataChanged += async (_, _) => await LoadAsync();
    }

    public IEnumerable<BusinessDocument> FilteredDocuments
    {
        get
        {
            IEnumerable<BusinessDocument> query = Documents;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(d =>
                    d.Number.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    d.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (d.Customer?.DisplayName ?? d.CustomerDisplayName ?? string.Empty)
                        .Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            query = SelectedFilter switch
            {
                "Invoices" => query.Where(d => d.Type == DocumentType.Invoice),
                "Estimates" => query.Where(d => d.Type == DocumentType.Estimate),
                "Draft" => query.Where(d => d.Status == DocumentStatus.Draft),
                "Sent" => query.Where(d => d.Status == DocumentStatus.Sent),
                "Paid" => query.Where(d => d.Status == DocumentStatus.Paid),
                "Overdue" => query.Where(d => d.Status == DocumentStatus.Overdue),
                "Cancelled" => query.Where(d => d.Status == DocumentStatus.Cancelled),
                _ => query
            };

            query = SelectedSort switch
            {
                "Oldest" => query.OrderBy(d => d.IssueDate),
                "Customer A-Z" => query.OrderBy(d => d.Customer?.DisplayName ?? d.CustomerDisplayName),
                "Total High-Low" => query.OrderByDescending(d => d.GrandTotal),
                "Total Low-High" => query.OrderBy(d => d.GrandTotal),
                _ => query.OrderByDescending(d => d.IssueDate)
            };

            return query.ToList();
        }
    }

    partial void OnSearchTextChanged(string value) => OnPropertyChanged(nameof(FilteredDocuments));
    partial void OnSelectedSortChanged(string value) => OnPropertyChanged(nameof(FilteredDocuments));
    partial void OnSelectedFilterChanged(string value) => OnPropertyChanged(nameof(FilteredDocuments));

    [RelayCommand]
    public async Task LoadAsync()
    {
        var list = await _repository.GetDocumentsAsync();

        Documents.Clear();
        foreach (var item in list)
            Documents.Add(item);

        OnPropertyChanged(nameof(FilteredDocuments));
    }

    [RelayCommand]
    public async Task DeleteAsync(BusinessDocument document)
    {
        if (document is null)
            return;

        bool confirm = await Shell.Current.DisplayAlert(
            "Delete document",
            $"Delete {document.Number}?",
            "Delete",
            "Cancel");

        if (!confirm)
            return;

        await _repository.DeleteDocumentAsync(document);
    }

    [RelayCommand]
    public async Task ExportPdfAsync(BusinessDocument document)
    {
        if (document is null)
            return;

        try
        {
            var branding = await _repository.GetBrandingAsync();

            // Replace this with your new Android-safe PDF generator later
            var bytes = _pdfExportService.GeneratePdf(document, branding);

            var tempPath = Path.Combine(FileSystem.CacheDirectory, $"{document.Number}.pdf");
            await File.WriteAllBytesAsync(tempPath, bytes);

            using var stream = File.OpenRead(tempPath);
            var saveResult = await FileSaver.Default.SaveAsync($"{document.Number}.pdf", stream);

            if (!saveResult.IsSuccessful)
                throw saveResult.Exception ?? new InvalidOperationException("Failed to save PDF.");

            await Shell.Current.DisplayAlert("Saved", $"Saved {document.Number}.pdf", "OK");

            // Optional share after save:
            // await Share.Default.RequestAsync(new ShareFileRequest
            // {
            //     Title = "Share PDF",
            //     File = new ShareFile(tempPath)
            // });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Export failed", ex.Message, "OK");
        }
    }

    [RelayCommand]
    public async Task ConvertToInvoiceAsync(BusinessDocument document)
    {
        if (document is null || document.Type != DocumentType.Estimate)
            return;

        var invoice = document.Clone();
        invoice.Id = Guid.NewGuid().ToString("N");
        invoice.Type = DocumentType.Invoice;
        invoice.Number = $"INV-{DateTime.Now:yyyyMMddHHmmss}";
        invoice.IssueDate = DateTime.Today;
        invoice.DueDate = DateTime.Today.AddDays(14);
        invoice.ValidUntil = null;
        invoice.Status = DocumentStatus.Draft;
        invoice.SyncPersistenceFields();

        await _repository.SaveDocumentAsync(invoice);
        await Shell.Current.DisplayAlert("Done", $"Created invoice {invoice.Number}.", "OK");
    }

    public bool CanConvert(BusinessDocument? document) =>
        document?.Type == DocumentType.Estimate;
}