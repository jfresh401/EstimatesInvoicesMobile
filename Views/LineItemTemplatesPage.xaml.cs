using FreshEstimate.Mobile.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FreshEstimate.Mobile.Views;

public partial class LineItemTemplatesPage : ContentPage
{
    private readonly LineItemTemplatesViewModel _viewModel;

    public LineItemTemplatesPage()
    {
        InitializeComponent();
        _viewModel = MauiProgram.Current.Services.GetRequiredService<LineItemTemplatesViewModel>();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}