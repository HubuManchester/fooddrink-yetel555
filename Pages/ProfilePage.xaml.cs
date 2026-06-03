using Fooddrink.Models;
using Fooddrink.Services;

namespace Fooddrink.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly SettingsService _settings;
    private readonly HardwareService _hardware;
    private readonly DatabaseService _db;
    private readonly JsonStorageService _json;

    public ProfilePage(SettingsService settings, HardwareService hardware, DatabaseService db, JsonStorageService json)
    {
        InitializeComponent();
        _settings = settings;
        _hardware = hardware;
        _db = db;
        _json = json;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var scale = _settings.FontScale;
        FontSlider.Value = scale;
        UpdatePreview(scale);
        LblJsonPath.Text = $"File: {_json.GetStoragePath()}";
        FontScalingHelper.ApplyScale(this, scale);
        await LoadFavoritesAsync();
        await LoadRecentViewsAsync();
    }

    private async Task LoadFavoritesAsync()
    {
        try
        {
            var favorites = await _db.GetFavoriteItemsAsync();
            ProfileFavoritesCollection.ItemsSource = null;
            ProfileFavoritesCollection.ItemsSource = favorites;
        }
        catch { }
    }

    private async Task LoadRecentViewsAsync()
    {
        try
        {
            var views = await _json.LoadRecentViewsAsync();
            RecentViewsCollection.ItemsSource = null;
            RecentViewsCollection.ItemsSource = views;
        }
        catch { }
    }

    private async void OnFavoriteTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not FoodItem item) return;

        try { _hardware.TriggerHapticFeedback(); } catch { }

        await Shell.Current.GoToAsync(nameof(DetailPage), true,
            new Dictionary<string, object> { ["FoodItem"] = item });
    }

    private async void OnRecentTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not RecentView view) return;

        try { _hardware.TriggerHapticFeedback(); } catch { }

        var item = await _db.GetFoodItemAsync(view.FoodItemId);
        if (item is not null)
        {
            await Shell.Current.GoToAsync(nameof(DetailPage), true,
                new Dictionary<string, object> { ["FoodItem"] = item });
        }
    }

    private void OnFontSliderChanged(object? sender, ValueChangedEventArgs e)
    {
        var scale = Math.Round(e.NewValue, 1);
        _settings.FontScale = scale;
        UpdatePreview(scale);
        FontScalingHelper.ApplyScale(this, scale);
        try { _hardware.TriggerHapticFeedback(); } catch { }
    }

    private void UpdatePreview(double scale)
    {
        LblScaleValue.Text = $"Font Scale: {scale:F1}x";
        LblPreviewTitle.FontSize = 16 * scale;
        LblPreviewBody.FontSize = 12 * scale;
    }

    private void OnResetFont(object? sender, EventArgs e)
    {
        _settings.ResetToDefault();
        FontSlider.Value = 1.0;
        UpdatePreview(1.0);
        FontScalingHelper.ApplyScale(this, 1.0);
        try { _hardware.TriggerHapticFeedback(); } catch { }
    }
}
