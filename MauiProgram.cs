using CommunityToolkit.Maui;
using FreshEstimate.Mobile.Converters;
using FreshEstimate.Mobile.Services;
using FreshEstimate.Mobile.ViewModels;
using FreshEstimate.Mobile.Views;

namespace FreshEstimate.Mobile;

public static class MauiProgram
{
    public static MauiApp Current { get; private set; } = null!;

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<AppRepository>();
        builder.Services.AddSingleton<CsvCustomerService>();
        builder.Services.AddSingleton<PdfExportService>();
        builder.Services.AddSingleton<EmbeddedAppLogoService>();

        builder.Services.AddSingleton<CurrencyConverter>();
        builder.Services.AddSingleton<StatusColorConverter>();
        builder.Services.AddSingleton<DocumentTypeToEstimateVisibilityConverter>();

        builder.Services.AddSingleton<DashboardViewModel>();
        builder.Services.AddSingleton<CustomersViewModel>();
        builder.Services.AddSingleton<DocumentsViewModel>();
        builder.Services.AddSingleton<BrandingViewModel>();
        builder.Services.AddTransient<CustomerEditorViewModel>();
        builder.Services.AddTransient<DocumentEditorViewModel>();

        builder.Services.AddSingleton<DashboardPage>();
        builder.Services.AddSingleton<CustomersPage>();
        builder.Services.AddSingleton<DocumentsPage>();
        builder.Services.AddSingleton<BrandingPage>();
        builder.Services.AddTransient<CustomerEditorPage>();
        builder.Services.AddTransient<DocumentEditorPage>();
        builder.Services.AddSingleton<LineItemTemplatesViewModel>();
        builder.Services.AddSingleton<LineItemTemplatesPage>();

        var app = builder.Build();
        Current = app;
        return app;
    }
}