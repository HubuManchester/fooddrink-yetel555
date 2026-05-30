using Fooddrink.Models;
using Fooddrink.Services;

namespace Fooddrink.Pages;

[QueryProperty(nameof(FoodItem), "FoodItem")]
public partial class DetailPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly HardwareService _hardware;
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

    public DetailPage(DatabaseService db, HardwareService hardware)
    {
        InitializeComponent();
        _db = db;
        _hardware = hardware;
    }

    private void PopulateUI(FoodItem item)
    {
        FoodName.Text = item.Name;
        FoodCategory.Text = $"Category: {item.Category}";
        FoodDescription.Text = item.Description;
        FoodIngredients.Text = item.Ingredients;
        NutritionCal.Text = $"{item.Calories:F0}";
        NutritionProtein.Text = $"{item.Protein:F0}g";
        NutritionCarbs.Text = $"{item.Carbs:F0}g";
        NutritionFat.Text = $"{item.Fat:F0}g";

        BtnFavorite.Text = item.IsFavorite ? "♥" : "♡";
        BtnFavorite.TextColor = item.IsFavorite ? Colors.Red : null;
        FoodImage.Source = item.ImageUrl;

        SemanticScreenReader.Announce($"Showing details for {item.Name}");
    }

    private async void OnSpeakClicked(object? sender, EventArgs e)
    {
        if (_foodItem is null) return;

        try
        {
            // Check if TTS engine is available
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            if (locales is null || !locales.Any())
            {
                await DisplayAlert("Text-to-Speech",
                    "No text-to-speech engine found. Please install 'Google Text-to-Speech' from the Play Store.", "OK");
                return;
            }

            _hardware.TriggerHapticFeedback();

            var ttsText = $"{_foodItem.Name}. It is a {_foodItem.Category} dish. " +
                          $"Calories: {_foodItem.Calories:F0} kilocalories. " +
                          $"Protein: {_foodItem.Protein:F0} grams. " +
                          $"Carbs: {_foodItem.Carbs:F0} grams. " +
                          $"Fat: {_foodItem.Fat:F0} grams. " +
                          $"Ingredients: {_foodItem.Ingredients}. " +
                          $"{_foodItem.Description}";

            await TextToSpeech.Default.SpeakAsync(ttsText);
        }
        catch (Exception)
        {
            await DisplayAlert("Text-to-Speech",
                "TTS engine failed. Install 'Google Text-to-Speech' from the Play Store to enable this feature.", "OK");
        }
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
