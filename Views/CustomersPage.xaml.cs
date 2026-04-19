using FreshEstimate.Mobile.Models;
using FreshEstimate.Mobile.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FreshEstimate.Mobile.Views;

public partial class CustomersPage : ContentPage
{
    private readonly CustomersViewModel _viewModel;
    private readonly IServiceProvider _services;

    public CustomersPage()
    {
        InitializeComponent();
        _services = MauiProgram.Current.Services;
        _viewModel = _services.GetRequiredService<CustomersViewModel>();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }

    private async void AddCustomer_Clicked(object? sender, EventArgs e)
    {
        var page = _services.GetRequiredService<CustomerEditorPage>();
        page.Initialize(null);
        await Navigation.PushModalAsync(new NavigationPage(page));
    }

    private async void EditCustomer_Invoked(object? sender, EventArgs e)
    {
        if (sender is SwipeItem item && item.CommandParameter is Customer customer)
        {
            var page = _services.GetRequiredService<CustomerEditorPage>();
            page.Initialize(customer);
            await Navigation.PushModalAsync(new NavigationPage(page));
        }
    }

    private async void DeleteCustomer_Invoked(object? sender, EventArgs e)
    {
        if (sender is SwipeItem item && item.CommandParameter is Customer customer)
        {
            bool confirm = await DisplayAlert("Delete customer", $"Delete {customer.DisplayName}?", "Delete", "Cancel");
            if (confirm)
                await _viewModel.DeleteCustomerCommand.ExecuteAsync(customer);
        }
    }
}