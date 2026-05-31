using System.Globalization;
using System.Windows.Data;

namespace ZooShop.Converters;

/// <summary>
/// Возвращает символ-иконку (Segoe Fluent Icons) для индикатора срока годности.
/// Просрочено → предупреждение-ошибка, скоро → предупреждение, норма → ничего.
/// </summary>
[ValueConversion(typeof(DateTime?), typeof(string))]
public sealed class ExpirationIconConverter : IValueConverter
{
    public static readonly ExpirationIconConverter Instance = new();

    // Segoe Fluent Icons: ErrorBadge (\uE783), Warning (\uE7BA)
    private const string IconError   = "\uE783";
    private const string IconWarning = "\uE7BA";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTime date) return string.Empty;
        if (date.Date < DateTime.Today) return IconError;
        if (date.Date <= DateTime.Today.AddDays(7)) return IconWarning;
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
