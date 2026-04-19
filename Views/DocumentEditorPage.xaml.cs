using FreshEstimate.Mobile.Models;
using FreshEstimate.Mobile.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FreshEstimate.Mobile.Views;

public partial class DocumentEditorPage : ContentPage
{
    private readonly DocumentEditorViewModel _viewModel;

    public DocumentEditorPage()
    {
        InitializeComponent();
        _viewModel = MauiProgram.Current.Services.GetRequiredService<DocumentEditorViewModel>();
        BindingContext = _viewModel;
    }

    public Task InitializeAsync(BusinessDocument? document, DocumentType? newType)
    {
        return _viewModel.LoadAsync(document, newType);
    }

    private void RemoveLine_Clicked(object? sender, EventArgs e)
    {
        if (sender is BindableObject bindable && bindable.BindingContext is LineItem item)
            _viewModel.RemoveLineCommand.Execute(item);
    }
}