using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using ZooShop.Data;
using ZooShop.Services;
using ZooShop.ViewModels;
using ZooShop.Views;
using ZooShop.Views.Pages;

namespace ZooShop;

/// <summary>
/// Точка входа приложения.
/// Настраивает DI-контейнер (Microsoft.Extensions.Hosting),
/// регистрирует все сервисы, ViewModel'и и View.
/// </summary>
public partial class App : Application
{
    private IHost _host = null!;

    public static IServiceProvider Services { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(cfg => cfg
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false))
            .ConfigureServices((ctx, svc) => ConfigureServices(ctx, svc))
            .Build();

        await _host.StartAsync();
        Services = _host.Services;

        // Автоматически создать / обновить БД и заполнить начальными данными
        await InitializeDatabaseAsync();

        // Применить тему WPF-UI
        Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
            Wpf.Ui.Appearance.ApplicationTheme.Light);

        // Открыть окно входа
        var loginWindow = Services.GetRequiredService<LoginWindow>();
        loginWindow.Show();
    }

    private static void ConfigureServices(HostBuilderContext ctx, IServiceCollection services)
    {
        // ── База данных ──────────────────────────────────────────────
        var connStr = ctx.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Строка подключения не найдена в appsettings.json");

        services.AddDbContextFactory<ZooShopDbContext>(options =>
            options.UseSqlServer(connStr));

        // ── Сервисы ──────────────────────────────────────────────────
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddTransient<IReportService, ReportService>();

        // ── ViewModels ───────────────────────────────────────────────
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ProductsViewModel>();
        services.AddTransient<AnimalsViewModel>();
        services.AddTransient<SalesViewModel>();
        services.AddTransient<StaffViewModel>();
        services.AddTransient<DeliveriesViewModel>();
        services.AddTransient<ReportsViewModel>();

        // ── Окна ─────────────────────────────────────────────────────
        services.AddTransient<LoginWindow>();
        services.AddTransient<MainWindow>();

        // ── Страницы ─────────────────────────────────────────────────
        services.AddTransient<DashboardPage>();
        services.AddTransient<ProductsPage>();
        services.AddTransient<AnimalsPage>();
        services.AddTransient<SalesPage>();
        services.AddTransient<StaffPage>();
        services.AddTransient<DeliveriesPage>();
        services.AddTransient<ReportsPage>();
    }

    private async Task InitializeDatabaseAsync()
    {
        try
        {
            var factory = Services.GetRequiredService<IDbContextFactory<ZooShopDbContext>>();
            await using var db = await factory.CreateDbContextAsync();
            await Data.DatabaseInitializer.InitializeAsync(db);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Не удалось подключиться к базе данных:\n\n{ex.Message}\n\n" +
                "Проверьте строку подключения в appsettings.json.\n" +
                "Для LocalDB убедитесь что установлен SQL Server Express.",
                "Ошибка БД",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }
}
