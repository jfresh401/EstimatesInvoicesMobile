using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreshEstimate.Mobile.Models;
using FreshEstimate.Mobile.Services;

namespace FreshEstimate.Mobile.ViewModels;

public partial class LineItemTemplatesViewModel : ObservableObject
{
    private readonly AppRepository _repository;

    public ObservableCollection<LineItemTemplate> Templates { get; } = new();

    [ObservableProperty] private LineItemTemplate? selectedTemplate;
    [ObservableProperty] private string editName = string.Empty;
    [ObservableProperty] private string editDescription = string.Empty;
    [ObservableProperty] private decimal editDefaultQuantity = 1m;
    [ObservableProperty] private decimal editDefaultUnitPrice = 0m;
    [ObservableProperty] private decimal editDefaultTaxRatePercent = 0m;

    public LineItemTemplatesViewModel(AppRepository repository)
    {
        _repository = repository;
    }

    partial void OnSelectedTemplateChanged(LineItemTemplate? value)
    {
        if (value is null)
        {
            EditName = string.Empty;
            EditDescription = string.Empty;
            EditDefaultQuantity = 1m;
            EditDefaultUnitPrice = 0m;
            EditDefaultTaxRatePercent = 0m;
            return;
        }

        EditName = value.Name;
        EditDescription = value.Description;
        EditDefaultQuantity = value.DefaultQuantity;
        EditDefaultUnitPrice = value.DefaultUnitPrice;
        EditDefaultTaxRatePercent = value.DefaultTaxRatePercent;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        Templates.Clear();

        var templates = await _repository.GetTemplatesAsync();
        foreach (var template in templates)
            Templates.Add(template);
    }

    [RelayCommand]
    private async Task AddTemplateAsync()
    {
        var template = new LineItemTemplate
        {
            Name = "New Template",
            Description = "New Template",
            DefaultQuantity = 1m,
            DefaultUnitPrice = 0m,
            DefaultTaxRatePercent = 0m
        };

        await _repository.SaveTemplateAsync(template);
        Templates.Add(template);
        SelectedTemplate = template;
    }

    [RelayCommand]
    private async Task SaveTemplateAsync()
    {
        if (SelectedTemplate is null)
            return;

        if (string.IsNullOrWhiteSpace(EditName))
        {
            await Shell.Current.DisplayAlert("Missing name", "Template name is required.", "OK");
            return;
        }

        SelectedTemplate.Name = EditName;
        SelectedTemplate.Description = EditDescription;
        SelectedTemplate.DefaultQuantity = EditDefaultQuantity;
        SelectedTemplate.DefaultUnitPrice = EditDefaultUnitPrice;
        SelectedTemplate.DefaultTaxRatePercent = EditDefaultTaxRatePercent;

        await _repository.SaveTemplateAsync(SelectedTemplate);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task DeleteTemplateAsync()
    {
        if (SelectedTemplate is null)
            return;

        bool confirm = await Shell.Current.DisplayAlert(
            "Delete template",
            $"Delete \"{SelectedTemplate.Name}\"?",
            "Delete",
            "Cancel");

        if (!confirm)
            return;

        await _repository.DeleteTemplateAsync(SelectedTemplate);
        SelectedTemplate = null;
        await LoadAsync();
    }
}