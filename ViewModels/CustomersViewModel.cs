using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreshEstimate.Mobile.Models;
using FreshEstimate.Mobile.Services;
using System.Collections.ObjectModel;

namespace FreshEstimate.Mobile.ViewModels;

public partial class CustomersViewModel : ObservableObject
{
    private readonly AppRepository _repository;
    private readonly CsvCustomerService _csvService;

    public ObservableCollection<Customer> Customers { get; } = new();

    [ObservableProperty]
    private Customer? selectedCustomer;

    [ObservableProperty]
    private string searchText = string.Empty;

    public CustomersViewModel(AppRepository repository, CsvCustomerService csvService)
    {
        _repository = repository;
        _csvService = csvService;
        _repository.DataChanged += async (_, _) => await LoadAsync();
    }

    public IEnumerable<Customer> FilteredCustomers =>
        string.IsNullOrWhiteSpace(SearchText)
            ? Customers.OrderBy(x => x.DisplayName)
            : Customers.Where(x =>
                    x.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    x.ContactName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    x.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.DisplayName);

    partial void OnSearchTextChanged(string value) => OnPropertyChanged(nameof(FilteredCustomers));

    [RelayCommand]
    public async Task LoadAsync()
    {
        var list = await _repository.GetCustomersAsync();
        Customers.Clear();
        foreach (var item in list)
            Customers.Add(item);
        OnPropertyChanged(nameof(FilteredCustomers));
    }

    [RelayCommand]
    public async Task DeleteCustomerAsync(Customer customer)
    {
        if (customer is null)
            return;

        var docs = await _repository.GetDocumentsAsync();
        if (docs.Any(d => d.CustomerId == customer.Id))
        {
            await Shell.Current.DisplayAlert("Delete blocked", "That customer is still assigned to one or more documents.", "OK");
            return;
        }

        await _repository.DeleteCustomerAsync(customer);
    }

    [RelayCommand]
    public async Task ExportCsvAsync()
    {
        try
        {
            await _csvService.ExportAsync(Customers);
            await Shell.Current.DisplayAlert("Export complete", "Customer CSV exported successfully.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Export failed", ex.Message, "OK");
        }
    }

    [RelayCommand]
    public async Task ImportCsvAsync()
    {
        try
        {
            var imported = await _csvService.ImportAsync();
            if (imported is null)
                return;

            var existing = await _repository.GetCustomersAsync();

            int added = 0;
            int updated = 0;

            foreach (var customer in imported)
            {
                var match = existing.FirstOrDefault(x =>
                    x.DisplayName.Equals(customer.DisplayName, StringComparison.OrdinalIgnoreCase) &&
                    x.Email.Equals(customer.Email, StringComparison.OrdinalIgnoreCase));

                if (match is null)
                {
                    await _repository.SaveCustomerAsync(customer);
                    added++;
                }
                else
                {
                    match.ContactName = customer.ContactName;
                    match.Phone = customer.Phone;
                    match.BillingAddress = customer.BillingAddress;
                    match.Notes = customer.Notes;
                    await _repository.SaveCustomerAsync(match);
                    updated++;
                }
            }

            await Shell.Current.DisplayAlert("Import complete", $"Added: {added}\nUpdated: {updated}", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Import failed", ex.Message, "OK");
        }
    }
}
