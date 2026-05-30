using Fooddrink.Models;
using SQLite;

namespace Fooddrink.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection _database = null!;
    private readonly string _dbPath;

    public DatabaseService()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "fooddrink.db3");
    }

    private async Task InitAsync()
    {
        if (_database is not null)
            return;

        try
        {
            _database = new SQLiteAsyncConnection(_dbPath,
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

            await _database.CreateTableAsync<FoodItem>();
            await _database.CreateTableAsync<ScanHistory>();

            var count = await _database.Table<FoodItem>().CountAsync();
            if (count == 0)
                await SeedDataAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Database Error",
                $"Could not initialize database: {ex.Message}", "OK");
        }
    }

    private async Task SeedDataAsync()
    {
        var items = new List<FoodItem>
        {
            new() { Name = "Peking Duck", Category = "Chinese", Description = "Crispy roasted duck served with thin pancakes, spring onions, and hoisin sauce. A world-famous Beijing specialty.", ImageUrl = "peking_duck.png", Ingredients = "Duck, hoisin sauce, spring onions, pancakes, cucumber", Calories = 550, Protein = 35, Carbs = 25, Fat = 35 },
            new() { Name = "Margherita Pizza", Category = "Italian", Description = "Classic Neapolitan pizza topped with San Marzano tomatoes, fresh mozzarella, and basil.", ImageUrl = "pizza.png", Ingredients = "Flour, tomatoes, mozzarella, basil, olive oil", Calories = 800, Protein = 30, Carbs = 90, Fat = 30 },
            new() { Name = "Sushi Platter", Category = "Japanese", Description = "Assorted fresh sushi including salmon, tuna, and eel nigiri with wasabi and pickled ginger.", ImageUrl = "sushi.jpg", Ingredients = "Rice, salmon, tuna, eel, nori, wasabi, ginger", Calories = 450, Protein = 40, Carbs = 50, Fat = 8 },
            new() { Name = "Pad Thai", Category = "Thai", Description = "Stir-fried rice noodles with shrimp, bean sprouts, eggs, and crushed peanuts in tamarind sauce.", ImageUrl = "pad_thai.png", Ingredients = "Rice noodles, shrimp, eggs, peanuts, tamarind, bean sprouts", Calories = 620, Protein = 28, Carbs = 70, Fat = 22 },
            new() { Name = "Butter Chicken", Category = "Indian", Description = "Tender chicken in a creamy tomato-based curry sauce, best served with naan bread or basmati rice.", ImageUrl = "butter_chicken.png", Ingredients = "Chicken, butter, cream, tomatoes, garam masala, garlic, ginger", Calories = 700, Protein = 45, Carbs = 25, Fat = 45 },
            new() { Name = "Fish and Chips", Category = "British", Description = "Golden-battered cod fillets served with thick-cut chips and tartar sauce.", ImageUrl = "fish_chips.png", Ingredients = "Cod, flour, potatoes, vegetable oil, tartar sauce", Calories = 850, Protein = 40, Carbs = 80, Fat = 42 },
            new() { Name = "Tacos al Pastor", Category = "Mexican", Description = "Corn tortillas filled with spit-roasted pork, pineapple, cilantro, and onion.", ImageUrl = "tacos.jpeg", Ingredients = "Pork, pineapple, corn tortillas, cilantro, onion, lime", Calories = 500, Protein = 35, Carbs = 40, Fat = 20 },
            new() { Name = "Croissant", Category = "French", Description = "Buttery, flaky crescent-shaped pastry. A classic French breakfast staple.", ImageUrl = "croissant.png", Ingredients = "Flour, butter, yeast, milk, sugar, egg", Calories = 350, Protein = 6, Carbs = 30, Fat = 23 },
            new() { Name = "Caesar Salad", Category = "American", Description = "Crisp romaine lettuce tossed with Caesar dressing, croutons, and Parmesan cheese.", ImageUrl = "caesar_salad.png", Ingredients = "Romaine lettuce, Parmesan, croutons, egg, anchovy, lemon", Calories = 320, Protein = 15, Carbs = 18, Fat = 22 },
            new() { Name = "Bibimbap", Category = "Korean", Description = "Mixed rice bowl topped with vegetables, beef, a fried egg, and gochujang chili paste.", ImageUrl = "bibimbap.png", Ingredients = "Rice, beef, spinach, carrots, egg, gochujang, sesame oil", Calories = 580, Protein = 32, Carbs = 65, Fat = 18 },
            new() { Name = "Pho Bo", Category = "Vietnamese", Description = "Vietnamese beef noodle soup with herbs, bean sprouts, and rich bone broth simmered for hours.", ImageUrl = "pho_bo.png", Ingredients = "Rice noodles, beef, star anise, cinnamon, ginger, fish sauce, bean sprouts, basil", Calories = 420, Protein = 35, Carbs = 45, Fat = 10 },
            new() { Name = "Paella Valenciana", Category = "Spanish", Description = "Traditional Spanish saffron rice dish with seafood, chicken, and vegetables.", ImageUrl = "paella.png", Ingredients = "Bomba rice, shrimp, mussels, chicken, saffron, bell peppers, peas", Calories = 650, Protein = 38, Carbs = 70, Fat = 22 },
            new() { Name = "Miso Ramen", Category = "Japanese", Description = "Rich miso-based ramen with chashu pork, soft-boiled egg, and fresh noodles.", ImageUrl = "miso_ramen.png", Ingredients = "Ramen noodles, miso paste, pork belly, egg, corn, nori, green onions", Calories = 720, Protein = 32, Carbs = 75, Fat = 30 },
            new() { Name = "Greek Moussaka", Category = "Greek", Description = "Layered eggplant casserole with spiced minced lamb and creamy bechamel topping.", ImageUrl = "moussaka.png", Ingredients = "Eggplant, ground lamb, tomato, onion, bechamel sauce, nutmeg, cheese", Calories = 560, Protein = 28, Carbs = 35, Fat = 35 },
            new() { Name = "Chicken Tikka Masala", Category = "Indian", Description = "Marinated chicken chunks in a spiced creamy tomato sauce, Britain's national dish.", ImageUrl = "tikka_masala.png", Ingredients = "Chicken, yogurt, cream, tomatoes, garam masala, cumin, coriander", Calories = 680, Protein = 42, Carbs = 30, Fat = 40 },
            new() { Name = "Chocolate Lava Cake", Category = "French", Description = "Warm chocolate cake with a molten center, served with vanilla ice cream.", ImageUrl = "lava_cake.png", Ingredients = "Dark chocolate, butter, eggs, sugar, flour, vanilla ice cream", Calories = 450, Protein = 8, Carbs = 48, Fat = 28 },
            new() { Name = "Vietnamese Spring Rolls", Category = "Vietnamese", Description = "Fresh rice paper rolls with shrimp, vermicelli, and herbs, served with peanut dipping sauce.", ImageUrl = "spring_rolls.png", Ingredients = "Rice paper, shrimp, vermicelli, lettuce, mint, peanut sauce", Calories = 180, Protein = 12, Carbs = 22, Fat = 5 },
            new() { Name = "Beef Stroganoff", Category = "Russian", Description = "Tender beef strips in a creamy mushroom sauce served over egg noodles.", ImageUrl = "stroganoff.png", Ingredients = "Beef sirloin, mushrooms, sour cream, onion, egg noodles, mustard", Calories = 620, Protein = 38, Carbs = 45, Fat = 32 },
            new() { Name = "Mango Sticky Rice", Category = "Thai", Description = "Sweet coconut sticky rice topped with ripe mango and coconut cream drizzle.", ImageUrl = "mango_rice.png", Ingredients = "Sticky rice, mango, coconut milk, sugar, salt, sesame seeds", Calories = 380, Protein = 5, Carbs = 65, Fat = 12 }
        };

        await _database.InsertAllAsync(items);
    }

    public async Task<List<FoodItem>> GetAllFoodItemsAsync()
    {
        await InitAsync();
        try { return await _database.Table<FoodItem>().ToListAsync(); }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load food items: {ex.Message}", "OK");
            return [];
        }
    }

    public async Task<List<FoodItem>> GetFoodItemsByCategoryAsync(string category)
    {
        await InitAsync();
        try
        {
            return await _database.Table<FoodItem>()
                .Where(f => f.Category == category)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to filter items: {ex.Message}", "OK");
            return [];
        }
    }

    public async Task<FoodItem?> GetFoodItemAsync(int id)
    {
        await InitAsync();
        try { return await _database.Table<FoodItem>().FirstOrDefaultAsync(f => f.Id == id); }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load food item: {ex.Message}", "OK");
            return null;
        }
    }

    public async Task<List<FoodItem>> SearchFoodItemsAsync(string query)
    {
        await InitAsync();
        try
        {
            return await _database.Table<FoodItem>()
                .Where(f => f.Name.Contains(query) || f.Category.Contains(query)
                    || f.Ingredients.Contains(query) || f.Description.Contains(query))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Search failed: {ex.Message}", "OK");
            return [];
        }
    }

    public async Task<List<FoodItem>> GetFavoriteItemsAsync()
    {
        await InitAsync();
        try
        {
            return await _database.Table<FoodItem>()
                .Where(f => f.IsFavorite)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load favorites: {ex.Message}", "OK");
            return [];
        }
    }

    public async Task ToggleFavoriteAsync(int foodItemId)
    {
        await InitAsync();
        try
        {
            var item = await _database.Table<FoodItem>().FirstOrDefaultAsync(f => f.Id == foodItemId);
            if (item is not null)
            {
                item.IsFavorite = !item.IsFavorite;
                await _database.UpdateAsync(item);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to update favorite: {ex.Message}", "OK");
        }
    }

    public async Task SaveScanAsync(string barcode, string foodName, string nutritionInfo)
    {
        await InitAsync();
        try
        {
            await _database.InsertAsync(new ScanHistory
            {
                Barcode = barcode,
                FoodName = foodName,
                NutritionInfo = nutritionInfo,
                ScanDate = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to save scan: {ex.Message}", "OK");
        }
    }

    public async Task<List<ScanHistory>> GetScanHistoryAsync()
    {
        await InitAsync();
        try
        {
            return await _database.Table<ScanHistory>()
                .OrderByDescending(s => s.ScanDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load scan history: {ex.Message}", "OK");
            return [];
        }
    }
}
