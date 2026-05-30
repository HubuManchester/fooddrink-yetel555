using Fooddrink.Models;
using Fooddrink.Services;

namespace Fooddrink.Pages;

public partial class FavoritesPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly HardwareService _hardware;

    public FavoritesPage(DatabaseService db, HardwareService hardware)
    {
        InitializeComponent();
        _db = db;
        _hardware = hardware;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadFavoritesAsync();
    }

    private async Task LoadFavoritesAsync()
    {
        try
        {
            var favorites = await _db.GetFavoriteItemsAsync();
            FavoritesCollection.ItemsSource = null;
            FavoritesCollection.ItemsSource = favorites;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load favorites: {ex.Message}", "OK");
        }
    }

    private async void OnFavoriteSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not FoodItem item) return;

        FavoritesCollection.SelectedItem = null;
        try { _hardware.TriggerHapticFeedback(); } catch { }

        await Shell.Current.GoToAsync(nameof(DetailPage), true,
            new Dictionary<string, object> { ["FoodItem"] = item });
    }

    private async void OnRemoveFavorite(object? sender, EventArgs e)
    {
        if (sender is not SwipeItem swipeItem) return;
        if (swipeItem.CommandParameter is not int foodId) return;

        try
        {
            _hardware.TriggerHapticFeedback();
            await _db.ToggleFavoriteAsync(foodId);

            SemanticScreenReader.Announce("Item removed from favorites");
            await LoadFavoritesAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not remove favorite: {ex.Message}", "OK");
        }
    }
}
