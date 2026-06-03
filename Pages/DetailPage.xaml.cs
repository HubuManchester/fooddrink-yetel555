using Fooddrink.Models;
using Fooddrink.Services;

namespace Fooddrink.Pages;

[QueryProperty(nameof(FoodItem), "FoodItem")]
public partial class DetailPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly HardwareService _hardware;
    private readonly SettingsService _settings;
    private readonly JsonStorageService _json;
    private FoodItem? _foodItem;

    public FoodItem? FoodItem
    {
        get => _foodItem;
        set
        {
            _foodItem = value;
            if (_foodItem is not null)
                PopulateUI(_foodItem);
        }
    }

    public DetailPage(DatabaseService db, HardwareService hardware, SettingsService settings, JsonStorageService json)
    {
        InitializeComponent();
        _db = db;
        _hardware = hardware;
        _settings = settings;
        _json = json;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _settings.FontScaleChanged += OnFontScaleChanged;
        if (_foodItem is not null)
        {
            FontScalingHelper.ApplyScale(this, _settings.FontScale);
            await _json.AddRecentViewAsync(_foodItem.Id, _foodItem.Name);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _settings.FontScaleChanged -= OnFontScaleChanged;
    }

    private void OnFontScaleChanged(object? sender, double scale)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            FontScalingHelper.ApplyScale(this, scale);
        });
    }

    private void PopulateUI(FoodItem item)
    {
        FoodName.Text = item.Name;
        FoodCategory.Text = $"Category: {item.Category}";
        FoodDescription.Text = item.Description;
        FoodIngredients.Text = item.Ingredients;
        FoodCookingMethod.Text = item.CookingMethod;
        NutritionCal.Text = $"{item.Calories:F0}";
        NutritionProtein.Text = $"{item.Protein:F0}g";
        NutritionCarbs.Text = $"{item.Carbs:F0}g";
        NutritionFat.Text = $"{item.Fat:F0}g";

        BtnFavorite.Text = item.IsFavorite ? "♥" : "♡";
        BtnFavorite.TextColor = item.IsFavorite ? Colors.Red : null;
        FoodImage.Source = item.ImageUrl;

        SemanticScreenReader.Announce($"Showing details for {item.Name}");
    }

    private async void OnFavoriteClicked(object? sender, EventArgs e)
    {
        if (_foodItem is null) return;

        try
        {
            _hardware.TriggerHapticFeedback();
            await _db.ToggleFavoriteAsync(_foodItem.Id);
            _foodItem.IsFavorite = !_foodItem.IsFavorite;
            BtnFavorite.Text = _foodItem.IsFavorite ? "♥" : "♡";
            BtnFavorite.TextColor = _foodItem.IsFavorite ? Colors.Red : null;

            var status = _foodItem.IsFavorite ? "added to" : "removed from";
            await DisplayAlert("Favorites", $"{_foodItem.Name} {status} favorites!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not update favorite: {ex.Message}", "OK");
        }
    }
}
