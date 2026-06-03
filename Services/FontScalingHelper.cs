namespace Fooddrink.Services;

public static class FontScalingHelper
{
    private static readonly Dictionary<Element, double> _baseSizes = [];

    public static void ApplyScale(VisualElement root, double scale)
    {
        WalkTree(root, scale);
    }

    private static void WalkTree(Element element, double scale)
    {
        if (element is Label label)
        {
            if (!_baseSizes.TryGetValue(label, out var baseSize))
            {
                baseSize = label.FontSize;
                _baseSizes[label] = baseSize;
            }
            label.FontSize = baseSize * scale;
        }
        else if (element is Button button)
        {
            if (!_baseSizes.TryGetValue(button, out var baseSize))
            {
                baseSize = button.FontSize;
                _baseSizes[button] = baseSize;
            }
            button.FontSize = baseSize * scale;
        }
        else if (element is Entry entry)
        {
            if (!_baseSizes.TryGetValue(entry, out var baseSize))
            {
                baseSize = entry.FontSize;
                _baseSizes[entry] = baseSize;
            }
            entry.FontSize = baseSize * scale;
        }

        if (element is IVisualTreeElement visual)
        {
            foreach (var child in visual.GetVisualChildren())
            {
                if (child is Element childElement)
                    WalkTree(childElement, scale);
            }
        }
    }
}
