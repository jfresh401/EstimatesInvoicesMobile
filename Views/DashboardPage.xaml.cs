using FreshEstimate.Mobile.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FreshEstimate.Mobile.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;

    public DashboardPage()
    {
        InitializeComponent();
        _viewModel = MauiProgram.Current.Services.GetRequiredService<DashboardViewModel>();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}