using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreshEstimate.Mobile.Models;
using FreshEstimate.Mobile.Services;

namespace FreshEstimate.Mobile.ViewModels;

public partial class CustomerEditorViewModel : ObservableObject
{
    private readonly AppRepository _repository;

    [ObservableProperty]
    private Customer customer = new();

    public string PageTitle => string.IsNullOrWhiteSpace(Customer?.DisplayName) ? "Customer" : Customer.DisplayName;

    public CustomerEditorViewModel(AppRepository repository)
    {
        _repository = repository;
    }

    public void Load(Customer? customer)
    {
        Customer = customer is null
            ? new Customer { Id = Guid.NewGuid().ToString() }
            : new Customer
            {
                Id = customer.Id,
                DisplayName = customer.DisplayName,
                ContactName = customer.ContactName,
                Email = customer.Email,
                Phone = customer.Phone,
                BillingAddress = customer.BillingAddress,
                Notes = customer.Notes
            };

        OnPropertyChanged(nameof(PageTitle));
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Customer.DisplayName))
        {
            await Shell.Current.DisplayAlert("Missing name", "Customer or business name is required.", "OK");
            return;
        }

        await _repository.SaveCustomerAsync(Customer);
        await Shell.Current.Navigation.PopModalAsync();
    }

    [RelayCommand]
    public async Task CancelAsync()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }
}
