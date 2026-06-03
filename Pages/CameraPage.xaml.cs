using Fooddrink.Services;

namespace Fooddrink.Pages;

public partial class CameraPage : ContentPage
{
    private readonly HardwareService _hardware;
    private readonly SettingsService _settings;

    public CameraPage(HardwareService hardware, SettingsService settings)
    {
        InitializeComponent();
        _hardware = hardware;
        _settings = settings;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _settings.FontScaleChanged += OnFontScaleChanged;
        FontScalingHelper.ApplyScale(this, _settings.FontScale);
        UpdateFlashUI();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _settings.FontScaleChanged -= OnFontScaleChanged;
    }

    private void OnFontScaleChanged(object? sender, double scale)
    {
        MainThread.BeginInvokeOnMainThread(() => FontScalingHelper.ApplyScale(this, scale));
    }

    private void UpdateFlashUI()
    {
        if (_hardware.IsFlashOn)
        {
            FlashStatus.Text = "Flashlight is ON";
            BtnFlash.Text = "Flash OFF";
            BtnFlash.BackgroundColor = Colors.OrangeRed;
        }
        else
        {
            FlashStatus.Text = "Flashlight is OFF";
            BtnFlash.Text = "Flash ON";
            BtnFlash.BackgroundColor = (Color)Application.Current!.Resources["Primary"];
        }
    }

    private async void OnToggleFlash(object? sender, EventArgs e)
    {
        try
        {
            _hardware.TriggerHapticFeedback();
            BtnFlash.IsEnabled = false;

            var turnOn = !_hardware.IsFlashOn;
            var (success, message) = await _hardware.ToggleFlashlightAsync(turnOn);

            if (success)
            {
                UpdateFlashUI();
                SemanticScreenReader.Announce(message);
            }
            else
            {
                await DisplayAlert("Flashlight", message, "OK");
                FlashStatus.Text = message;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Flashlight Error", $"Could not control flashlight: {ex.Message}", "OK");
        }
        finally
        {
            BtnFlash.IsEnabled = true;
        }
    }

    private async void OnTakePhoto(object? sender, EventArgs e)
    {
        try
        {
            _hardware.TriggerHapticFeedback();

            var result = await _hardware.TakePhotoAsync();

            if (result.StartsWith("/") || result.StartsWith("C:") || File.Exists(result))
            {
                PhotoPreview.Source = ImageSource.FromFile(result);
                SemanticScreenReader.Announce("Photo captured successfully");
                await DisplayAlert("Success", "Photo captured successfully!", "OK");
            }
            else
            {
                await DisplayAlert("Camera", result, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Camera Error", $"Could not take photo: {ex.Message}", "OK");
        }
    }

    private async void OnPickPhoto(object? sender, EventArgs e)
    {
        try
        {
            _hardware.TriggerHapticFeedback();

            var result = await _hardware.PickPhotoAsync();

            if (result.StartsWith("/") || result.StartsWith("C:") || File.Exists(result))
            {
                PhotoPreview.Source = ImageSource.FromFile(result);
                SemanticScreenReader.Announce("Photo selected from gallery");
            }
            else
            {
                await DisplayAlert("Gallery", result, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Gallery Error", $"Could not pick photo: {ex.Message}", "OK");
        }
    }
}
