using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreshEstimate.Mobile.Models;
using FreshEstimate.Mobile.Services;

namespace FreshEstimate.Mobile.ViewModels;

public partial class BrandingViewModel : ObservableObject
{
    private readonly AppRepository _repository;

    [ObservableProperty]
    private BrandingSettings branding = new();

    public BrandingViewModel(AppRepository repository)
    {
        _repository = repository;
        _repository.DataChanged += async (_, _) => await LoadAsync();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        Branding = await _repository.GetBrandingAsync();
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        await _repository.SaveBrandingAsync(Branding);
        await Shell.Current.DisplayAlert("Saved", "Branding settings saved.", "OK");
    }

    [RelayCommand]
    public async Task PickLogoAsync()
    {
        var file = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select logo image"
        });

        if (file is null)
            return;

        var dest = Path.Combine(FileSystem.AppDataDirectory, Path.GetFileName(file.FileName));
        await using var src = await file.OpenReadAsync();
        await using var dst = File.Create(dest);
        await src.CopyToAsync(dst);

        Branding.LogoFilePath = dest;
        await _repository.SaveBrandingAsync(Branding);
    }

    [RelayCommand]
    public async Task ClearLogoAsync()
    {
        Branding.LogoFilePath = null;
        await _repository.SaveBrandingAsync(Branding);
    }
}
