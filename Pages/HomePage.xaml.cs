using Fooddrink.Models;
using Fooddrink.Services;

namespace Fooddrink.Pages;

public partial class HomePage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly HardwareService _hardware;
    private readonly SettingsService _settings;
    private List<FoodItem> _allItems = [];
    private FoodItem? _recommendedItem;
    private string _currentCategory = "All";
    private bool _isShaking;
    private static readonly Random _rng = new();

    public HomePage(DatabaseService db, HardwareService hardware, SettingsService settings)
    {
        InitializeComponent();
        _db = db;
        _hardware = hardware;
        _settings = settings;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _settings.FontScaleChanged += OnFontScaleChanged;
        FontScalingHelper.ApplyScale(this, _settings.FontScale);
        await LoadItemsAsync(null);

        if (_allItems.Count > 0)
            SetRecommended(_allItems[0]);

        _hardware.StartShakeDetection(OnShakeDetected);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _hardware.StopShakeDetection();
        _settings.FontScaleChanged -= OnFontScaleChanged;
    }

    private void OnFontScaleChanged(object? sender, double scale)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            FontScalingHelper.ApplyScale(this, scale);
            if (_recommendedItem is not null)
                SetRecommended(_recommendedItem);
            FoodCollectionView.ItemsSource = null;
            FoodCollectionView.ItemsSource = _allItems;
        });
    }

    private async Task LoadItemsAsync(string? category)
    {
        try
        {
            _allItems = category is null or "All"
                ? await _db.GetAllFoodItemsAsync()
                : await _db.GetFoodItemsByCategoryAsync(category);

            FoodCollectionView.ItemsSource = null;
            FoodCollectionView.ItemsSource = _allItems;

            // Delay to let CollectionView create items, then scale them
            await Task.Delay(100);
            FontScalingHelper.ApplyScale(this, _settings.FontScale);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load food items: {ex.Message}", "OK");
        }
    }

    private void SetRecommended(FoodItem item)
    {
        _recommendedItem = item;
        RecommendName.Text = item.Name;
        RecommendCategory.Text = item.Category;
        RecommendCalories.Text = $"{item.Calories:F0} kcal  |  Protein: {item.Protein:F0}g";
        RecommendDesc.Text = item.Description;
        RecommendImage.Source = item.ImageUrl;

        SemanticScreenReader.Announce($"Recommended: {item.Name}");
    }

    private async void OnRecommendTapped(object? sender, TappedEventArgs e)
    {
        if (_recommendedItem is null) return;

        try { _hardware.TriggerHapticFeedback(); } catch { }

        await Shell.Current.GoToAsync(nameof(DetailPage), true,
            new Dictionary<string, object> { ["FoodItem"] = _recommendedItem });
    }

    private void PickRandomRecommendation()
    {
        if (_allItems.Count == 0) return;

        var candidates = _allItems.Where(i => i != _recommendedItem).ToList();
        if (candidates.Count == 0) candidates = _allItems;

        var randomItem = candidates[_rng.Next(candidates.Count)];
        SetRecommended(randomItem);

        RecommendHint.Text = "🎲 A new recommendation — shake again!";
    }

    private async void OnCategoryClicked(object? sender, EventArgs e)
    {
        if (sender is not Button btn) return;

        _currentCategory = btn.Text;

        // Reset all buttons to default
        foreach (var child in ((HorizontalStackLayout)btn.Parent).Children)
        {
            if (child is Button b)
            {
                b.BackgroundColor = (Color)Application.Current!.Resources["Primary"];
                b.TextColor = Colors.White;
            }
        }

        // Highlight selected
        btn.BackgroundColor = (Color)Application.Current!.Resources["Tertiary"];

        try { _hardware.TriggerHapticFeedback(); } catch { }
        await LoadItemsAsync(_currentCategory);
    }

    private async void OnFoodTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not FoodItem item) return;

        try { _hardware.TriggerHapticFeedback(); } catch { }

        await Shell.Current.GoToAsync(nameof(DetailPage), true,
            new Dictionary<string, object> { ["FoodItem"] = item });
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadItemsAsync(_currentCategory);
        foodRefreshView.IsRefreshing = false;
    }

    private void OnShakeDetected()
    {
        if (_isShaking) return;
        _isShaking = true;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            try { _hardware.TriggerHapticFeedback(); } catch { }
            PickRandomRecommendation();
            _isShaking = false;
        });
    }
}
