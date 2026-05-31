using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ZooShop.Behaviors;

/// <summary>
/// Attached behavior: форматирует поле ввода как +7 (XXX) XXX-XX-XX по мере набора.
/// Использование: b:PhoneMaskBehavior.IsActive="True"
/// </summary>
public static class PhoneMaskBehavior
{
    private static readonly HashSet<TextBox> _busy = new();

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.RegisterAttached(
            "IsActive", typeof(bool), typeof(PhoneMaskBehavior),
            new PropertyMetadata(false, OnIsActiveChanged));

    public static bool GetIsActive(DependencyObject d)  => (bool)d.GetValue(IsActiveProperty);
    public static void SetIsActive(DependencyObject d, bool v) => d.SetValue(IsActiveProperty, v);

    private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox tb) return;
        if ((bool)e.NewValue)
        {
            tb.PreviewTextInput += OnPreviewTextInput;
            tb.TextChanged      += OnTextChanged;
        }
        else
        {
            tb.PreviewTextInput -= OnPreviewTextInput;
            tb.TextChanged      -= OnTextChanged;
        }
    }

    // Блокируем нецифровые символы
    private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (!e.Text.All(char.IsDigit)) e.Handled = true;
    }

    // Форматируем при любом изменении (ввод, backspace, delete, вставка)
    private static void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        var tb = (TextBox)sender;
        if (!_busy.Add(tb)) return;   // предотвращаем рекурсию
        try
        {
            var digits    = ExtractDigits(tb.Text);
            var formatted = FormatPhone(digits);
            if (tb.Text == formatted) return;
            tb.Text        = formatted;
            tb.CaretIndex  = formatted.Length;
        }
        finally { _busy.Remove(tb); }
    }

    private static string ExtractDigits(string s)
    {
        var d = new string(s.Where(char.IsDigit).ToArray());
        if (d.Length > 0 && (d[0] == '7' || d[0] == '8')) d = d[1..];
        return d.Length > 10 ? d[..10] : d;
    }

    private static string FormatPhone(string d) => d.Length switch
    {
        0  => string.Empty,
        <= 3 => $"+7 ({d}",
        <= 6 => $"+7 ({d[..3]}) {d[3..]}",
        <= 8 => $"+7 ({d[..3]}) {d[3..6]}-{d[6..]}",
        _    => $"+7 ({d[..3]}) {d[3..6]}-{d[6..8]}-{d[8..]}"
    };
}
