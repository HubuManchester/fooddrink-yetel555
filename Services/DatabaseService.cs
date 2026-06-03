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
            new() { Name = "Peking Duck", Category = "Chinese", Description = "Crispy roasted duck — Beijing's imperial dish.", ImageUrl = "peking_duck.png", Ingredients = "Whole duck, hoisin sauce, Chinese five-spice powder, maltose syrup, Chinkiang black vinegar, scallions, cucumber strips, Mandarin thin pancakes", CookingMethod = "Inflate duck skin with air pump, blanch and air-dry for 24h until parchment-like, brush with maltose-vinegar glaze, hang-roast in wood-fired oven at 200°C for 60 min until lacquered mahogany", Calories = 550, Protein = 35, Carbs = 25, Fat = 35 },
            new() { Name = "Margherita Pizza", Category = "Italian", Description = "The original Neapolitan pizza — simplicity perfected.", ImageUrl = "pizza.png", Ingredients = "Tipo 00 fine wheat flour, San Marzano plum tomatoes, fresh mozzarella di bufala Campana, Genovese basil leaves, extra virgin olive oil, Sicilian sea salt, active dry yeast", CookingMethod = "Hand-knead dough for 15 min, proof at room temp 8h, stretch into thin disc by hand — never roll, top with hand-crushed tomato and torn mozzarella, bake at 485°C in wood-fired oven for exactly 90 seconds", Calories = 800, Protein = 30, Carbs = 90, Fat = 30 },
            new() { Name = "Sushi Platter", Category = "Japanese", Description = "Assorted nigiri and maki — Edo-style craftsmanship.", ImageUrl = "sushi.jpg", Ingredients = "Koshihikari short-grain sushi rice, sashimi-grade salmon, bluefin tuna akami, unagi freshwater eel, toasted nori sheets, rice vinegar blend, real wasabi root, gari pickled ginger, artisan soy sauce", CookingMethod = "Wash rice in 7 changes of cold water, cook in seasoned kombu dashi, fan-cool while folding in vinegar mixture, hand-form nigiri in single smooth motion, slice neta at 45° angle against grain, paint unagi with tare glaze and torch-finish", Calories = 450, Protein = 40, Carbs = 50, Fat = 8 },
            new() { Name = "Pad Thai", Category = "Thai", Description = "Thailand's iconic stir-fried noodle — sweet, sour, savory.", ImageUrl = "pad_thai.png", Ingredients = "Chanthaburi rice stick noodles, Gulf tiger shrimp, free-range eggs, crisp bean sprouts, roasted peanuts crushed, tamarind concentrate, nam pla fish sauce, palm sugar, garlic chives, fresh lime, prik bon chili flakes", CookingMethod = "Soak noodles in warm water 30 min until pliable, fire up wok to smoking hot, flash-fry shrimp and minced shallot, crack eggs and scramble at edge of wok, add drained noodles and tamarind-fish sauce blend, toss furiously at maximum heat for 3 min until noodles char slightly at edges", Calories = 620, Protein = 28, Carbs = 70, Fat = 22 },
            new() { Name = "Butter Chicken", Category = "Indian", Description = "Murgh makhani — velvety tomato-butter curry from Delhi.", ImageUrl = "butter_chicken.png", Ingredients = "Bone-in chicken thighs, Amul butter, heavy cream, tomato puree, onion, garlic, ginger, garam masala blend, cumin seeds, coriander powder, turmeric, dried fenugreek leaves, hung yogurt, lemon juice", CookingMethod = "Marinate chicken in spiced yogurt 4h minimum, thread onto skewers and cook in tandoor at 480°C until charred at edges, shred tandoori meat into makhani gravy (butter-tomato-cream base simmered with fenugreek), finish with a swirl of cream and butter", Calories = 700, Protein = 45, Carbs = 25, Fat = 45 },
            new() { Name = "Fish and Chips", Category = "British", Description = "Crispy battered cod with hand-cut chips — a British pub classic.", ImageUrl = "fish_chips.png", Ingredients = "North Atlantic cod loin, Maris Piper potatoes, plain flour, cold lager beer, baking powder, sea salt flakes, Sarson's malt vinegar, beef dripping for deep-frying, homemade tartar sauce, mushy peas", CookingMethod = "Hand-cut potatoes into 2cm thick batons, first fry in beef dripping at 130°C until soft but pale, rest and cool, second fry at 185°C until golden and glass-crisp, dip cod in cold beer batter, deep-fry at 180°C for 6 min until batter puffs and crackles", Calories = 850, Protein = 40, Carbs = 80, Fat = 42 },
            new() { Name = "Tacos al Pastor", Category = "Mexican", Description = "Trompo-roasted pork with pineapple — Mexico City street legend.", ImageUrl = "tacos.jpeg", Ingredients = "Pork shoulder butt, fresh pineapple, achiote paste, guajillo chilies, white vinegar, garlic, Mexican oregano, nixtamal corn tortillas, white onion, cilantro, lime wedges, salsa verde", CookingMethod = "Marinate pork in achiote-chile adobo overnight until deep red, stack on vertical rotating trompo with whole pineapple on top, slow-roast for 2h while outer layer chars, shave thin crispy-edged slices directly onto warm tortillas, garnish with diced onion and cilantro", Calories = 500, Protein = 35, Carbs = 40, Fat = 20 },
            new() { Name = "Croissant", Category = "French", Description = "Buttery laminated Viennoiserie — 729 layers of perfection.", ImageUrl = "croissant.png", Ingredients = "T55 bread flour, Lescure butter (82% butterfat), whole milk, caster sugar, fresh yeast, Guérande sea salt, whole egg", CookingMethod = "Create détrempe dough, encase cold butter block, perform 3 single turns with 45 min rest between each to create 27 layers of butter, rest overnight, roll to 3.5mm thickness, cut isosceles triangles, roll from base to tip, crescent shaping, proof at 26°C for 2h, double egg wash, bake at 195°C with steam for 16 min", Calories = 350, Protein = 6, Carbs = 30, Fat = 23 },
            new() { Name = "Caesar Salad", Category = "American", Description = "Tableside-prepared classic from Tijuana, not Rome.", ImageUrl = "caesar_salad.png", Ingredients = "Whole romaine hearts, aged Parmigiano-Reggiano wedge, rustic sourdough croutons, pasteurized egg yolk, oil-packed anchovy fillets, Dijon mustard, fresh garlic clove, Meyer lemon, extra virgin olive oil, Worcestershire sauce, cracked black pepper", CookingMethod = "Crush garlic into wooden salad bowl with salt to form paste, whisk in egg yolk, anchovy and mustard, drizzle olive oil in thin stream while whisking to emulsify, toss whole romaine leaves gently with dressing, shave Parmesan over with vegetable peeler, scatter toasted croutons", Calories = 320, Protein = 15, Carbs = 18, Fat = 22 },
            new() { Name = "Bibimbap", Category = "Korean", Description = "Sizzling stone bowl of rice with jewel-toned toppings.", ImageUrl = "bibimbap.png", Ingredients = "Korean short-grain rice, beef ribeye bulgogi, spinach namul, carrot julienne, zucchini rounds, pyogo shiitake, soybean sprouts, sunny-side egg, gochujang chili paste, toasted sesame oil, guk-ganjang soy sauce, crushed garlic, toasted sesame seeds", CookingMethod = "Sauté each vegetable individually with sesame oil and salt to preserve distinct color and flavor, sear bulgogi-marinated beef at high heat, heat stone dolsot bowl until smoking, coat with sesame oil, pack in hot rice, arrange toppings in color-wheel pattern, crown with egg yolk, serve with gochujang for diner to mix vigorously", Calories = 580, Protein = 32, Carbs = 65, Fat = 18 },
            new() { Name = "Pho Bo", Category = "Vietnamese", Description = "Hanoi-style beef noodle soup — clear broth, deep soul.", ImageUrl = "pho_bo.png", Ingredients = "Fresh bánh phở flat rice noodles, beef brisket, oxtail, beef marrow bones, star anise, cassia cinnamon bark, charred ginger, grilled onion, fish sauce, yellow rock sugar, bean sprouts, Thai basil, lime wedges, hoisin sauce, sriracha chili sauce", CookingMethod = "Char onion and ginger over open flame until blackened and fragrant, blanch bones and parboil to remove impurities, simmer bones with charred aromatics and spice sachet for 8 hours, skim constantly for crystal-clear broth, blanch noodles for 10 seconds, paper-thin raw beef slices on top, ladle boiling broth over to cook beef instantly", Calories = 420, Protein = 35, Carbs = 45, Fat = 10 },
            new() { Name = "Paella Valenciana", Category = "Spanish", Description = "Authentic Valencian paella — saffron, smoke, and socarrat.", ImageUrl = "paella.png", Ingredients = "Bomba rice from Calasparra, free-range chicken, wild rabbit, flat green beans, garrofó white beans, ripe grated tomato, saffron threads, pimentón de la Vera smoked paprika, fresh rosemary sprig, extra virgin olive oil, rich chicken stock", CookingMethod = "Build fire under wide shallow paella pan, sear meat in olive oil until deeply browned, push to edges and sofrito tomato and beans in center, stir in rice to toast grains in oil, pour boiling stock infused with saffron, arrange ingredients, cook uncovered 18-20 min without stirring — listen for the crackle of socarrat crust forming at the bottom", Calories = 650, Protein = 38, Carbs = 70, Fat = 22 },
            new() { Name = "Miso Ramen", Category = "Japanese", Description = "Sapporo-style miso ramen — rich, hearty, warming.", ImageUrl = "miso_ramen.png", Ingredients = "Sapporo-style curly ramen noodles, shiro white miso, chashu braised pork belly, ajitama marinated soft-boiled egg, Hokkaido sweet corn, butter pat, nori sheets, green onions, garlic, ginger, tonkotsu pork bone broth, toasted sesame oil, rayu chili oil", CookingMethod = "Simmer pork bones for 12+ hours until milky white, dissolve miso into ladle of hot broth just before serving (never boil miso), cook noodles 30 seconds under al dente, torch chashu slices until caramelized, assemble bowl with precision, crown with butter pat that slowly melts into broth", Calories = 720, Protein = 32, Carbs = 75, Fat = 30 },
            new() { Name = "Greek Moussaka", Category = "Greek", Description = "Layered eggplant casserole — the taste of a Greek taverna.", ImageUrl = "moussaka.png", Ingredients = "Large eggplants, ground lamb, yellow onion, garlic, canned plum tomatoes, cinnamon stick, allspice berries, grated nutmeg, waxy potatoes, butter, all-purpose flour, whole milk, kefalotyri cheese, eggs", CookingMethod = "Salt eggplant slices to draw out bitterness, rinse and pat dry, pan-fry until golden in olive oil, separately brown lamb with tomato and warming spices, parboil potato slices, layer in ceramic dish: potato base → eggplant → meat → eggplant, blanketed with thick flour-butter-milk bechamel enriched with eggs and cheese, bake at 180°C for 50 min until bechamel dome is puffed and bronzed", Calories = 560, Protein = 28, Carbs = 35, Fat = 35 },
            new() { Name = "Chicken Tikka Masala", Category = "Indian", Description = "Char-grilled chicken tikka in spiced cream sauce.", ImageUrl = "tikka_masala.png", Ingredients = "Chicken breast supremes, Greek yogurt, heavy cream, tomato puree, onion, garlic, ginger, garam masala, cumin, coriander, turmeric, Kashmiri degi mirch, dried fenugreek leaves, ghee butter", CookingMethod = "Cube chicken breast, marinate in spiced yogurt overnight, thread onto metal skewers, cook under fierce grill or tandoor until edges blacken and blister, simmer in velvety tomato-onion masala sauce with Kashmiri chili for vivid orange color, finish with fenugreek and a pour of cream", Calories = 680, Protein = 42, Carbs = 30, Fat = 40 },
            new() { Name = "Chocolate Lava Cake", Category = "French", Description = "Molten-centered chocolate dessert — dramatic and decadent.", ImageUrl = "lava_cake.png", Ingredients = "Valrhona dark chocolate (70% cacao), French butter, eggs, egg yolks, confectioners sugar, all-purpose flour, Dutch-process cocoa powder, Madagascar vanilla extract, fleur de sel, vanilla bean ice cream", CookingMethod = "Gently melt chocolate and butter over bain-marie (never microwave), whisk whole eggs, yolks and sugar until ribbon stage ribbons form, fold melted chocolate into egg foam with light hand, dust flour and fold just until no streaks remain, fill buttered-and-cocoa-dusted ramekins 3/4 full, bake at 220°C for exactly 12 min — edges set, center jiggles, invert immediately, dust with powdered snow", Calories = 450, Protein = 8, Carbs = 48, Fat = 28 },
            new() { Name = "Vietnamese Spring Rolls", Category = "Vietnamese", Description = "Gỏi cuốn — fresh, translucent, no cooking needed.", ImageUrl = "spring_rolls.png", Ingredients = "Bánh tráng rice paper rounds, poached prawns halved lengthwise, rice vermicelli, butter lettuce leaves, fresh mint, Thai basil, cilantro, bean sprouts, carrot matchsticks, hoisin sauce, creamy peanut butter, lime juice, crushed roasted peanuts", CookingMethod = "Dip rice paper in warm water for exactly 5 seconds — still slightly firm, lay flat on damp towel, build ingredients in neat horizontal line at lower third, fold bottom edge up over filling, fold sides inward like an envelope, roll forward tightly while pulling filling back, serve whole or bias-cut with hoisin-peanut dip", Calories = 180, Protein = 12, Carbs = 22, Fat = 5 },
            new() { Name = "Beef Stroganoff", Category = "Russian", Description = "Tender beef strips in tangy sour cream mushroom sauce.", ImageUrl = "stroganoff.png", Ingredients = "Beef tenderloin center-cut, cremini mushrooms, yellow onion, smetana sour cream, Dijon mustard, rich beef stock, unsalted butter, all-purpose flour, egg noodles, fresh dill fronds, black pepper, salt", CookingMethod = "Slice tenderloin across grain into thin strips, sear in batches in smoking-hot butter for 45 seconds per side — do not crowd the pan, remove beef and rest, sauté mushrooms and onion in same pan with meat fond, deglaze with beef stock, stir in sour cream and mustard off heat (prevent curdling), return beef to warm through, serve piled over buttered egg noodles", Calories = 620, Protein = 38, Carbs = 45, Fat = 32 },
            new() { Name = "Mango Sticky Rice", Category = "Thai", Description = "Khao niew mamuang — the queen of Thai desserts.", ImageUrl = "mango_rice.png", Ingredients = "Thai glutinous sticky rice, ripe Nam Dok Mai mangoes, coconut milk, palm sugar, white sugar, salt, pandan leaves, toasted sesame seeds, thick coconut cream", CookingMethod = "Soak sticky rice in cold water for at least 6h or overnight, drain and steam in bamboo basket over boiling water for 25 min until translucent and tender, heat coconut milk with palm sugar and knotted pandan leaves, fold sweet coconut cream into hot rice and let rest covered 20 min to absorb, slice mango into elegant fans, mold rice into dome alongside mango, drizzle with salted coconut cream", Calories = 380, Protein = 5, Carbs = 65, Fat = 12 }
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
            // Split query into keywords, search each across all fields
            var keywords = query.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                .Select(k => k.Trim().ToLower())
                                .Distinct()
                                .ToList();

            if (keywords.Count == 0) return [];

            // Fetch all items matched by any keyword across 5 fields
            var allItems = await _database.Table<FoodItem>().ToListAsync();

            return allItems.Where(item =>
            {
                var lowerName = item.Name.ToLower();
                var lowerCat = item.Category.ToLower();
                var lowerIng = item.Ingredients.ToLower();
                var lowerCook = item.CookingMethod.ToLower();
                var lowerDesc = item.Description.ToLower();

                return keywords.Any(kw =>
                    lowerName.Contains(kw) ||
                    lowerCat.Contains(kw) ||
                    lowerIng.Contains(kw) ||
                    lowerCook.Contains(kw) ||
                    lowerDesc.Contains(kw));
            }).ToList();
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
