using Fooddrink.Services;

namespace Fooddrink.Pages;

public partial class CameraPage : ContentPage
{
    private readonly HardwareService _hardware;

    public CameraPage(HardwareService hardware)
    {
        InitializeComponent();
        _hardware = hardware;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        FlashStatus.Text = "Checking flash...";
        try
        {
            FlashStatus.Text = _hardware.GetFlashStatus();
        }
        catch (Exception ex)
        {
            FlashStatus.Text = $"Flash error: {ex.Message}";
        }
    }

    private async void OnTakePhoto(object? sender, EventArgs e)
    {
        try
        {
            _hardware.TriggerHapticFeedback();

            BtnFlash.IsEnabled = false;
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
        finally
        {
            BtnFlash.IsEnabled = true;
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

    private async void OnFlashStatus(object? sender, EventArgs e)
    {
        try
        {
            var status = _hardware.GetFlashStatus();
            FlashStatus.Text = status;
            _hardware.TriggerHapticFeedback();
            await DisplayAlert("Flash Status", status, "OK");
        }
        catch (Exception ex)
        {
            FlashStatus.Text = $"Flash is not available";
            await DisplayAlert("Flash Error", $"Flash is not supported on this device: {ex.Message}", "OK");
        }
    }
}
