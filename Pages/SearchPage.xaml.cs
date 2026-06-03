using System.Collections.ObjectModel;
using Fooddrink.Models;
using Fooddrink.Services;

namespace Fooddrink.Pages;

public partial class SearchPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly HardwareService _hardware;
    private readonly SettingsService _settings;
    private readonly ObservableCollection<FoodItem> _searchResults = [];

    public SearchPage(DatabaseService db, HardwareService hardware, SettingsService settings)
    {
        InitializeComponent();
        _db = db;
        _hardware = hardware;
        _settings = settings;
        SearchResultsCollection.ItemsSource = _searchResults;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _settings.FontScaleChanged += OnFontScaleChanged;
        FontScalingHelper.ApplyScale(this, _settings.FontScale);
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

    private async void OnSearchCompleted(object? sender, EventArgs e)
    {
        var query = SearchEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            await DisplayAlert("Validation", "Please enter a search keyword.", "OK");
            return;
        }

        try
        {
            _hardware.TriggerHapticFeedback();
            var results = await _db.SearchFoodItemsAsync(query);

            _searchResults.Clear();
            foreach (var item in results)
                _searchResults.Add(item);

            if (results.Count == 0)
                await DisplayAlert("No Results", $"No food items match '{query}'.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Search failed: {ex.Message}", "OK");
        }
    }

    private async void OnVoiceSearch(object? sender, EventArgs e)
    {
        try
        {
            var result = await _hardware.StartVoiceRecognitionAsync();

            if (result.StartsWith("Voice recognition ready"))
            {
                string? input = await DisplayPromptAsync("Voice Search",
                    "Microphone is ready. Type your search below (or use voice dictation on your keyboard):",
                    "Search", "Cancel",
                    keyboard: Keyboard.Default);

                if (!string.IsNullOrWhiteSpace(input))
                {
                    SearchEntry.Text = input;
                    OnSearchCompleted(this, EventArgs.Empty);
                }
            }
            else
            {
                await DisplayAlert("Voice Input", result, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Voice Input Error", $"Could not access microphone: {ex.Message}", "OK");
        }
    }

    private async void OnResultTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not FoodItem item) return;

        try { _hardware.TriggerHapticFeedback(); } catch { }

        await Shell.Current.GoToAsync(nameof(DetailPage), true,
            new Dictionary<string, object> { ["FoodItem"] = item });
    }
}
