using Fooddrink.Services;

namespace Fooddrink.Pages;

public partial class LocationPage : ContentPage
{
    private readonly HardwareService _hardware;
    private CancellationTokenSource? _trackCts;

    public LocationPage(HardwareService hardware)
    {
        InitializeComponent();
        _hardware = hardware;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CheckLocationStatus();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopTracking();
    }

    private async Task CheckLocationStatus()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            LocationStatus.Text = status == PermissionStatus.Granted
                ? "Location permission is granted."
                : "Location permission required. Tap the button below.";
        }
        catch (Exception ex)
        {
            LocationStatus.Text = $"Could not check location: {ex.Message}";
        }
    }

    private async void OnGetLocation(object? sender, EventArgs e)
    {
        try
        {
            _hardware.TriggerHapticFeedback();
            var result = await _hardware.GetCurrentLocationAsync();

            if (result.StartsWith("Lat:"))
            {
                var parts = result.Split(", ");
                LatitudeLabel.Text = parts[0].Replace("Lat: ", "");
                LongitudeLabel.Text = parts[1].Replace("Lon: ", "");
                LocationTip.Text = "Location acquired successfully.";
                SemanticScreenReader.Announce($"Current location: {result}");
            }
            else
            {
                LatitudeLabel.Text = "--";
                LongitudeLabel.Text = "--";
                LocationStatus.Text = result;
            }
        }
        catch (Exception ex)
        {
            LatitudeLabel.Text = "Error";
            LongitudeLabel.Text = "Error";
            await DisplayAlert("Location Error", $"Could not get location: {ex.Message}", "OK");
        }
    }

    private async void OnStartTracking(object? sender, EventArgs e)
    {
        try
        {
            _trackCts = new CancellationTokenSource();
            _hardware.TriggerHapticFeedback();
            LocationStatus.Text = "Tracking active...";

            while (!_trackCts.Token.IsCancellationRequested)
            {
                var result = await _hardware.GetCurrentLocationAsync();
                if (result.StartsWith("Lat:"))
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        var parts = result.Split(", ");
                        LatitudeLabel.Text = parts[0].Replace("Lat: ", "");
                        LongitudeLabel.Text = parts[1].Replace("Lon: ", "");
                        LocationTip.Text = $"Last update: {DateTime.Now:HH:mm:ss}";
                    });
                }
                await Task.Delay(3000, _trackCts.Token);
            }
        }
        catch (TaskCanceledException) { }
        catch (Exception ex)
        {
            await DisplayAlert("Tracking Error", $"Could not track location: {ex.Message}", "OK");
        }
    }

    private void StopTracking()
    {
        try
        {
            _trackCts?.Cancel();
            _trackCts?.Dispose();
            _trackCts = null;
        }
        catch { }
    }
}
