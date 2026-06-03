namespace Fooddrink.Services;

public class SettingsService
{
    private const string FontScaleKey = "font_scale";
    private const double DefaultScale = 1.0;
    private const double MinScale = 0.8;
    private const double MaxScale = 1.8;

    public double FontScale
    {
        get => Preferences.Get(FontScaleKey, DefaultScale);
        set
        {
            var clamped = Math.Clamp(value, MinScale, MaxScale);
            Preferences.Set(FontScaleKey, clamped);
            FontScaleChanged?.Invoke(this, clamped);
        }
    }

    public event EventHandler<double>? FontScaleChanged;

    public double MinFontScale => MinScale;
    public double MaxFontScale => MaxScale;

    public void ResetToDefault()
    {
        FontScale = DefaultScale;
    }
}
