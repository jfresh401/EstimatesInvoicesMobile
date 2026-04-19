using FreshEstimate.Mobile.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FreshEstimate.Mobile.Views;

public partial class BrandingPage : ContentPage
{
    private readonly BrandingViewModel _viewModel;

    public BrandingPage()
    {
        InitializeComponent();
        _viewModel = MauiProgram.Current.Services.GetRequiredService<BrandingViewModel>();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}