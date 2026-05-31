using System.Windows;
using System.Windows.Controls;

namespace ZooShop.Services;

/// <summary>
/// Реализация навигационного сервиса на основе стандартного WPF Frame.
/// Singleton — регистрируется один раз в DI; Frame задаётся
/// из code-behind MainWindow после инициализации.
/// </summary>
public sealed class NavigationService : INavigationService
{
    private Frame? _frame;
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public void SetNavigationHost(System.Windows.Controls.Frame frame)
    {
        _frame = frame;
    }

    /// <inheritdoc/>
    public void Navigate<TPage>() where TPage : class
        => Navigate(typeof(TPage));

    /// <inheritdoc/>
    public void Navigate(Type pageType)
    {
        if (_frame is null)
            throw new InvalidOperationException(
                "Frame не зарегистрирован. Вызовите SetNavigationHost сначала.");

        // Разрешаем страницу через DI (чтобы ViewModel был инжектирован)
        var page = _serviceProvider.GetService(pageType)
                   ?? Activator.CreateInstance(pageType)
                   ?? throw new InvalidOperationException(
                       $"Не удалось создать страницу типа {pageType.Name}");

        Application.Current.Dispatcher.Invoke(() =>
            _frame.Navigate(page));
    }

    /// <inheritdoc/>
    public void GoBack()
    {
        if (_frame?.CanGoBack == true)
            Application.Current.Dispatcher.Invoke(() => _frame.GoBack());
    }
}
