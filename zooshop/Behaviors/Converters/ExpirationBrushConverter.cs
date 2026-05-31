using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ZooShop.Converters;

/// <summary>
/// Преобразует DateTime? срока годности в цветовую кисть.
/// Просрочено → красный, менее 7 дней → янтарный, нормально → прозрачный.
/// </summary>
[ValueConversion(typeof(DateTime?), typeof(Brush))]
public sealed class ExpirationBrushConverter : IValueConverter
{
    public static readonly ExpirationBrushConverter Instance = new();

    private static readonly SolidColorBrush Expired =
        new(Color.FromArgb(60, 220, 50, 50));

    private static readonly SolidColorBrush ExpiringSoon =
        new(Color.FromArgb(60, 255, 160, 0));

    private static readonly SolidColorBrush Normal =
        new(Colors.Transparent);

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTime date) return Normal;

        if (date.Date < DateTime.Today) return Expired;
        if (date.Date <= DateTime.Today.AddDays(7)) return ExpiringSoon;
        return Normal;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
