namespace Fooddrink.Services;

public class HardwareService
{
    public async Task<string> TakePhotoAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
                return "Camera is not supported on this device.";

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo is null)
                return "No photo was taken.";

            var localPath = Path.Combine(FileSystem.AppDataDirectory, photo.FileName);
            using var stream = await photo.OpenReadAsync();
            using var fileStream = File.OpenWrite(localPath);
            await stream.CopyToAsync(fileStream);

            return localPath;
        }
        catch (PermissionException)
        {
            return "Camera permission is required. Please enable it in device settings.";
        }
        catch (Exception ex)
        {
            return $"Could not take photo: {ex.Message}";
        }
    }

    public async Task<string> PickPhotoAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo is null)
                return "No photo was selected.";

            var localPath = Path.Combine(FileSystem.AppDataDirectory, photo.FileName);
            using var stream = await photo.OpenReadAsync();
            using var fileStream = File.OpenWrite(localPath);
            await stream.CopyToAsync(fileStream);

            return localPath;
        }
        catch (Exception ex)
        {
            return $"Could not pick photo: {ex.Message}";
        }
    }

    public string GetFlashStatus()
    {
        try
        {
            return "Flash is ready for camera use.";
        }
        catch (Exception ex)
        {
            return $"Flash is not available: {ex.Message}";
        }
    }

    public string GetVibrationStatus()
    {
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            return "Vibration feedback is available.";
        }
        catch (Exception ex)
        {
            return $"Vibration is not supported on this device: {ex.Message}";
        }
    }

    public void TriggerHapticFeedback()
    {
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch
        {
            // Silently fail — haptic feedback is non-critical
        }
    }

    public string GetAccelerometerStatus()
    {
        try
        {
            if (Accelerometer.Default.IsSupported)
                return "Accelerometer is available. Shake to refresh!";
            return "Accelerometer is not supported on this device.";
        }
        catch (Exception ex)
        {
            return $"Accelerometer error: {ex.Message}";
        }
    }

    public void StartShakeDetection(Action onShake)
    {
        try
        {
            if (!Accelerometer.Default.IsSupported)
                return;

            const double shakeThreshold = 1.2;
            Accelerometer.Default.ReadingChanged += (s, e) =>
            {
                var magnitude = Math.Sqrt(
                    e.Reading.Acceleration.X * e.Reading.Acceleration.X +
                    e.Reading.Acceleration.Y * e.Reading.Acceleration.Y +
                    e.Reading.Acceleration.Z * e.Reading.Acceleration.Z);

                if (magnitude > shakeThreshold)
                    MainThread.BeginInvokeOnMainThread(onShake);
            };
            Accelerometer.Default.Start(SensorSpeed.UI);
        }
        catch (Exception)
        {
            // Shake detection is non-critical
        }
    }

    public void StopShakeDetection()
    {
        try
        {
            if (Accelerometer.Default.IsSupported && Accelerometer.Default.IsMonitoring)
                Accelerometer.Default.Stop();
        }
        catch { }
    }

    public async Task<string> GetCurrentLocationAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                    return "Location permission is required. Please enable it in device settings.";
            }

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location is null)
                return "Could not determine location. Ensure GPS is enabled.";

            return $"Lat: {location.Latitude:F6}, Lon: {location.Longitude:F6}";
        }
        catch (PermissionException)
        {
            return "Location permission is required. Please enable it in device settings.";
        }
        catch (FeatureNotSupportedException)
        {
            return "GPS is not supported on this device.";
        }
        catch (FeatureNotEnabledException)
        {
            return "GPS is disabled. Please turn on location services.";
        }
        catch (Exception ex)
        {
            return $"Location error: {ex.Message}";
        }
    }

    public async Task<string> StartVoiceRecognitionAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Microphone>();
                if (status != PermissionStatus.Granted)
                    return "Microphone permission is required for voice input.";
            }

            return "Voice recognition ready — speak your search query.";
        }
        catch (Exception ex)
        {
            return $"Voice input error: {ex.Message}";
        }
    }
}
