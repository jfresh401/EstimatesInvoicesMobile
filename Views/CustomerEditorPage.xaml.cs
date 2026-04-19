using FreshEstimate.Mobile.Models;
using FreshEstimate.Mobile.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FreshEstimate.Mobile.Views;

public partial class CustomerEditorPage : ContentPage
{
    private readonly CustomerEditorViewModel _viewModel;

    public CustomerEditorPage()
    {
        InitializeComponent();
        _viewModel = MauiProgram.Current.Services.GetRequiredService<CustomerEditorViewModel>();
        BindingContext = _viewModel;
    }

    public void Initialize(Customer? customer)
    {
        _viewModel.Load(customer);
    }
}