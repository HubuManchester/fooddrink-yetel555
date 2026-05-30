# Food & Drink App

A cross-platform .NET MAUI application for browsing world cuisines, searching food items, managing favorites, capturing food photos, and scanning barcodes for nutrition information.

## Features

### Basic Features
- **Food Browser**: CollectionView displaying food items with category filtering (Chinese, Italian, Japanese, Thai, Indian, etc.)
- **Search**: Keyword search across food names, ingredients, categories, and descriptions
- **Favorites**: Add/remove food items to favorites with swipe-to-delete
- **Image Display**: Food photos loaded and displayed in list and detail views
- **Tab Navigation**: Bottom tab bar for quick access to all sections

### Advanced Features
- **Barcode Scanner**: Enter or scan barcodes to retrieve nutrition information
- **Voice Input**: Microphone-enabled voice search for hands-free food lookup
- **Camera Capture**: Take food photos or pick from gallery with flash support
- **Shake to Refresh**: Accelerometer-based shake detection to refresh food recommendations
- **Haptic Feedback**: Vibration feedback on button clicks and food card interactions

### Hardware Features (6 of 6)
| Hardware | Usage |
|----------|-------|
| Camera | Capture food photos |
| Flash | Camera illumination support |
| Microphone | Voice input for food search |
| Accelerometer | Shake detection to refresh food list |
| Vibration | Haptic feedback on interactions |
| Barometer | Display altitude/pressure for cooking adjustments |

### Data Storage
- **SQLite Database**: Local storage for food items, scan history, and favorites
- Tables: `FoodItems`, `ScanHistory`
- Pre-seeded with 10 international food items

### Accessibility
- Dark Mode support (system theme aware)
- Semantic screen reader descriptions on all interactive elements
- WCAG-compliant color contrast
- Minimum touch targets of 44x44 points
- Font scaling support via standard MAUI styles

## Project Structure

```
Fooddrink/
  Models/
    FoodItem.cs          # Food data model (SQLite table)
    ScanHistory.cs       # Barcode scan history model
  Services/
    DatabaseService.cs   # SQLite CRUD operations + seed data
    HardwareService.cs   # Hardware feature wrappers (camera, mic, sensors, etc.)
  Pages/
    HomePage.xaml/.cs    # Main food browser with categories
    DetailPage.xaml/.cs  # Food detail with nutrition info + favorite
    SearchPage.xaml/.cs  # Search + voice input + barcode scan
    FavoritesPage.xaml/.cs # Saved favorite items
    CameraPage.xaml/.cs  # Photo capture + gallery + flash
    BarometerPage.xaml/.cs # Pressure/altitude monitoring
  Services/
    DatabaseService.cs   # SQLite database operations
    HardwareService.cs   # Hardware abstraction layer
```

## Dependencies

- .NET 8.0
- Microsoft.Maui.Controls
- sqlite-net-pcl (1.9.172)
- SQLitePCLRaw.bundle_green (2.1.10)
- ZXing.Net.Maui.Controls (0.4.0)

## Error Handling

All features include comprehensive try-catch error handling with user-friendly messages:
- Camera not supported / permission denied
- Microphone permission required
- Flash not available on device
- Barometer sensor unavailable
- Accelerometer unsupported
- Vibration not available
- Database operation failures
- Input validation for empty search/barcode fields

## Deployment

### Android
1. Open `Fooddrink.sln` in Visual Studio 2022
2. Select Android target
3. Build and deploy to emulator or device

Required Android permissions are configured in `Platforms/Android/AndroidManifest.xml`:
- CAMERA, FLASHLIGHT, RECORD_AUDIO, VIBRATE, BODY_SENSORS

### Windows
1. Select Windows target in Visual Studio
2. Note: Some hardware features (camera, barometer) may be simulated on Windows

## Development

```bash
# Restore packages
dotnet restore

# Build
dotnet build

# Run (Android emulator required)
dotnet build -t:Run -f net8.0-android
```
