using System.Text.Json;

namespace Fooddrink.Services;

public class RecentView
{
    public int FoodItemId { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public DateTime ViewedAt { get; set; }
}

public class JsonStorageService
{
    private readonly string _filePath;
    private const int MaxRecentItems = 20;

    public JsonStorageService()
    {
        _filePath = Path.Combine(FileSystem.AppDataDirectory, "recent_views.json");
    }

    public async Task<List<RecentView>> LoadRecentViewsAsync()
    {
        try
        {
            if (!File.Exists(_filePath))
                return [];

            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<RecentView>>(json) ?? [];
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("JSON Storage Error",
                $"Could not load recent views: {ex.Message}", "OK");
            return [];
        }
    }

    public async Task AddRecentViewAsync(int foodItemId, string foodName)
    {
        try
        {
            var views = await LoadRecentViewsAsync();

            // Remove duplicate if exists and re-insert at top
            views.RemoveAll(v => v.FoodItemId == foodItemId);
            views.Insert(0, new RecentView
            {
                FoodItemId = foodItemId,
                FoodName = foodName,
                ViewedAt = DateTime.Now
            });

            // Keep only last N items
            if (views.Count > MaxRecentItems)
                views = views.Take(MaxRecentItems).ToList();

            var json = JsonSerializer.Serialize(views, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("JSON Storage Error",
                $"Could not save recent view: {ex.Message}", "OK");
        }
    }

    public string GetStoragePath() => _filePath;
}
