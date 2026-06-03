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

    private bool _isFlashOn;

    public Task<(bool Success, string Message)> ToggleFlashlightAsync(bool turnOn)
    {
        try
        {
#if ANDROID
            if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.M)
                return Task.FromResult((false, "Flashlight requires Android 6.0 or higher."));

            var context = Android.App.Application.Context;
            var cameraManager = (Android.Hardware.Camera2.CameraManager)
                context.GetSystemService(Android.Content.Context.CameraService)!;

            var cameraId = cameraManager.GetCameraIdList().FirstOrDefault(c =>
            {
                var obj = cameraManager.GetCameraCharacteristics(c)
                    .Get(Android.Hardware.Camera2.CameraCharacteristics.LensFacing);
                return obj is Java.Lang.Integer ji && ji.IntValue() == (int)Android.Hardware.Camera2.LensFacing.Back;
            });

            if (cameraId is null)
                return Task.FromResult((false, "No back camera with flash found on this device."));

            var flashObj = cameraManager.GetCameraCharacteristics(cameraId)
                .Get(Android.Hardware.Camera2.CameraCharacteristics.FlashInfoAvailable);
            var hasFlash = flashObj is Java.Lang.Boolean jb && jb.BooleanValue();
            if (!hasFlash)
                return Task.FromResult((false, "This device does not have a camera flash."));

#pragma warning disable CA1416
            cameraManager.SetTorchMode(cameraId, turnOn);
#pragma warning restore CA1416
            _isFlashOn = turnOn;
            return Task.FromResult((true, turnOn ? "Flashlight is ON" : "Flashlight is OFF"));
#else
            return Task.FromResult((false, "Flashlight control is only available on Android devices."));
#endif
        }
        catch (Exception ex)
        {
            _isFlashOn = false;
            return Task.FromResult((false, $"Flashlight error: {ex.Message}"));
        }
    }

    public bool IsFlashOn => _isFlashOn;

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

    private EventHandler<AccelerometerChangedEventArgs>? _shakeHandler;

    public void StartShakeDetection(Action onShake)
    {
        try
        {
            if (!Accelerometer.Default.IsSupported)
                return;

            const double shakeThreshold = 1.2;
            const int shakesRequired = 3;
            const int cooldownMs = 400;
            const int resetWindowMs = 2500;

            int shakeCount = 0;
            DateTime lastShakeTime = DateTime.MinValue;
            DateTime firstShakeTime = DateTime.MinValue;

            _shakeHandler = (s, e) =>
            {
                var magnitude = Math.Sqrt(
                    e.Reading.Acceleration.X * e.Reading.Acceleration.X +
                    e.Reading.Acceleration.Y * e.Reading.Acceleration.Y +
                    e.Reading.Acceleration.Z * e.Reading.Acceleration.Z);

                if (magnitude > shakeThreshold)
                {
                    var now = DateTime.UtcNow;

                    // Reset if too long since first shake
                    if (firstShakeTime != DateTime.MinValue &&
                        (now - firstShakeTime).TotalMilliseconds > resetWindowMs)
                    {
                        shakeCount = 0;
                        firstShakeTime = DateTime.MinValue;
                    }

                    // Cooldown between individual shakes
                    if (lastShakeTime != DateTime.MinValue &&
                        (now - lastShakeTime).TotalMilliseconds < cooldownMs)
                        return;

                    shakeCount++;
                    lastShakeTime = now;

                    if (firstShakeTime == DateTime.MinValue)
                        firstShakeTime = now;

                    if (shakeCount >= shakesRequired)
                    {
                        shakeCount = 0;
                        firstShakeTime = DateTime.MinValue;
                        MainThread.BeginInvokeOnMainThread(onShake);
                    }
                }
            };

            Accelerometer.Default.ReadingChanged += _shakeHandler;
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
            {
                if (_shakeHandler is not null)
                    Accelerometer.Default.ReadingChanged -= _shakeHandler;
                Accelerometer.Default.Stop();
                _shakeHandler = null;
            }
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
