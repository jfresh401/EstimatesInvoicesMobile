using FreshEstimate.Mobile.Models;
using FreshEstimate.Mobile.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FreshEstimate.Mobile.Views;

public partial class DocumentsPage : ContentPage
{
    private readonly DocumentsViewModel _viewModel;
    private readonly IServiceProvider _services;

    public DocumentsPage()
    {
        InitializeComponent();
        _services = MauiProgram.Current.Services;
        _viewModel = _services.GetRequiredService<DocumentsViewModel>();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }

    private async void NewEstimate_Clicked(object? sender, EventArgs e)
    {
        var page = _services.GetRequiredService<DocumentEditorPage>();
        await page.InitializeAsync(null, DocumentType.Estimate);
        await Navigation.PushModalAsync(new NavigationPage(page));
    }

    private async void NewInvoice_Clicked(object? sender, EventArgs e)
    {
        var page = _services.GetRequiredService<DocumentEditorPage>();
        await page.InitializeAsync(null, DocumentType.Invoice);
        await Navigation.PushModalAsync(new NavigationPage(page));
    }

    private async void EditDocument_Invoked(object? sender, EventArgs e)
    {
        if (sender is SwipeItem item && item.CommandParameter is BusinessDocument document)
        {
            var page = _services.GetRequiredService<DocumentEditorPage>();
            await page.InitializeAsync(document, null);
            await Navigation.PushModalAsync(new NavigationPage(page));
        }
    }

    private async void ConvertEstimate_Invoked(object? sender, EventArgs e)
    {
        if (sender is SwipeItem item && item.CommandParameter is BusinessDocument document)
            await _viewModel.ConvertToInvoiceCommand.ExecuteAsync(document);
    }

    private async void ExportPdf_Invoked(object? sender, EventArgs e)
    {
        if (sender is SwipeItem item && item.CommandParameter is BusinessDocument document)
            await _viewModel.ExportPdfCommand.ExecuteAsync(document);
    }

    private async void DeleteDocument_Invoked(object? sender, EventArgs e)
    {
        if (sender is SwipeItem item && item.CommandParameter is BusinessDocument document)
            await _viewModel.DeleteCommand.ExecuteAsync(document);
    }
}