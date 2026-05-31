using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ZooShop.Converters;

/// <summary>
/// Преобразует путь к файлу или URL в BitmapImage.
/// При пустом пути или ошибке возвращает null (XAML показывает Placeholder).
/// </summary>
[ValueConversion(typeof(string), typeof(BitmapImage))]
public sealed class ImagePathConverter : IValueConverter
{
    public static readonly ImagePathConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string path || string.IsNullOrWhiteSpace(path))
            return null;

        try
        {
            Uri uri;
            if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
            {
                uri = new Uri(path);
            }
            else
            {
                var full = Path.IsPathRooted(path)
                    ? path
                    : Path.Combine(AppContext.BaseDirectory, path);
                uri = new Uri(full);
            }

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource     = uri;
            bmp.CacheOption   = BitmapCacheOption.OnLoad;
            bmp.DecodePixelWidth = 200; // ограничение памяти
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        catch
        {
            return null; // Placeholder будет виден через Trigger
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
