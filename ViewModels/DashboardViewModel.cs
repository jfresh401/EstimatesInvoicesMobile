using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreshEstimate.Mobile.Models;
using FreshEstimate.Mobile.Services;
using System.Collections.ObjectModel;

namespace FreshEstimate.Mobile.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly AppRepository _repository;

    public ObservableCollection<BusinessDocument> RecentDocuments { get; } = new();

    [ObservableProperty]
    private int totalCustomers;

    [ObservableProperty]
    private int totalDocuments;

    [ObservableProperty]
    private decimal outstandingRevenue;

    [ObservableProperty]
    private decimal paidRevenue;

    [ObservableProperty]
    private decimal estimatePipeline;

    [ObservableProperty]
    private int overdueCount;

    public DashboardViewModel(AppRepository repository)
    {
        _repository = repository;
        _repository.DataChanged += async (_, _) => await LoadAsync();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var customers = await _repository.GetCustomersAsync();
        var documents = await _repository.GetDocumentsAsync();

        TotalCustomers = customers.Count;
        TotalDocuments = documents.Count;
        OutstandingRevenue = documents.Where(d => d.Type == DocumentType.Invoice && d.Status != DocumentStatus.Paid && d.Status != DocumentStatus.Cancelled).Sum(d => d.GrandTotal);
        PaidRevenue = documents.Where(d => d.Type == DocumentType.Invoice && d.Status == DocumentStatus.Paid).Sum(d => d.GrandTotal);
        EstimatePipeline = documents.Where(d => d.Type == DocumentType.Estimate && d.Status != DocumentStatus.Cancelled).Sum(d => d.GrandTotal);
        OverdueCount = documents.Count(d => d.Type == DocumentType.Invoice && d.Status == DocumentStatus.Overdue);

        RecentDocuments.Clear();
        foreach (var doc in documents.Take(10))
            RecentDocuments.Add(doc);
    }
}
