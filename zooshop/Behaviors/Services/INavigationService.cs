namespace ZooShop.Services;

/// <summary>
/// Контракт навигационного сервиса.
/// Позволяет ViewModels инициировать переходы между страницами
/// без прямых зависимостей на View.
/// </summary>
public interface INavigationService
{
    /// <summary>Навигировать на страницу по типу.</summary>
    void Navigate<TPage>() where TPage : class;

    /// <summary>Навигировать на страницу по типу (runtime).</summary>
    void Navigate(Type pageType);

    /// <summary>Вернуться на предыдущую страницу.</summary>
    void GoBack();

    /// <summary>Зарегистрировать Frame/NavigationView как хост.</summary>
    void SetNavigationHost(System.Windows.Controls.Frame frame);
}
