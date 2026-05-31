using System.Globalization;
using System.Windows.Data;

namespace ZooShop.Converters;

/// <summary>Пустая / null строка → false, непустая → true.</summary>
[ValueConversion(typeof(string), typeof(bool))]
public sealed class NullOrEmptyStringToBoolConverter : IValueConverter
{
    public static readonly NullOrEmptyStringToBoolConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !string.IsNullOrEmpty(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Инверсия boolean.</summary>
[ValueConversion(typeof(bool), typeof(bool))]
public sealed class InverseBoolConverter : IValueConverter
{
    public static readonly InverseBoolConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && !b;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && !b;
}
