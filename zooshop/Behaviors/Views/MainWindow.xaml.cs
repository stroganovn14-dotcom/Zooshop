using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ZooShop.Services;
using ZooShop.ViewModels;
using ZooShop.Views.Pages;

namespace ZooShop.Views;

public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
{
    private readonly INavigationService _nav;
    private readonly IAuthService _auth;
    private bool _isSwitchingUser;

    private static readonly Dictionary<string, Type> PageMap = new()
    {
        ["Dashboard"]  = typeof(DashboardPage),
        ["Products"]   = typeof(ProductsPage),
        ["Animals"]    = typeof(AnimalsPage),
        ["Deliveries"] = typeof(DeliveriesPage),
        ["Sales"]      = typeof(SalesPage),
        ["Staff"]      = typeof(StaffPage),
        ["Reports"]    = typeof(ReportsPage),
    };

    public MainWindow(MainViewModel vm, INavigationService nav, IAuthService auth)
    {
        InitializeComponent();
        DataContext = vm;
        _nav  = nav;
        _auth = auth;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _nav.SetNavigationHost(ContentFrame);
        _nav.Navigate<DashboardPage>();
    }

    private void NavItem_Clicked(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.RadioButton rb &&
            rb.Tag is string tag &&
            PageMap.TryGetValue(tag, out var pageType))
        {
            _nav.Navigate(pageType);
        }
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new LogoutDialog { Owner = this };
        dlg.ShowDialog();

        switch (dlg.Choice)
        {
            case LogoutChoice.SwitchUser:
                _auth.Logout();
                _isSwitchingUser = true;
                var login = App.Services.GetRequiredService<LoginWindow>();
                login.Show();
                Close();
                break;
            case LogoutChoice.ExitApp:
                Application.Current.Shutdown();
                break;
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1) DragMove();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!_isSwitchingUser)
            Application.Current.Shutdown();
    }

    private static void SetBrush(string key, string hex)
        => Application.Current.Resources[key] =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));

    private void ThemeGreen_Click(object sender, RoutedEventArgs e)
    {
        SetBrush("AppSidebarBg",     "#C8E6C9");
        SetBrush("AppNavHoverBg",    "#A5D6A7");
        SetBrush("AppNavSelectedBg", "#81C784");
        SetBrush("AppPageBg",        "#F1F8F2");
        SetBrush("AppAccentBrush",   "#4CAF50");
    }

    private void ThemeYellow_Click(object sender, RoutedEventArgs e)
    {
        SetBrush("AppSidebarBg",     "#FFF9C4");
        SetBrush("AppNavHoverBg",    "#FFF176");
        SetBrush("AppNavSelectedBg", "#FFD54F");
        SetBrush("AppPageBg",        "#FFFEF5");
        SetBrush("AppAccentBrush",   "#F9A825");
    }

    private void ThemeBlue_Click(object sender, RoutedEventArgs e)
    {
        SetBrush("AppSidebarBg",     "#BBDEFB");
        SetBrush("AppNavHoverBg",    "#90CAF9");
        SetBrush("AppNavSelectedBg", "#64B5F6");
        SetBrush("AppPageBg",        "#F0F7FF");
        SetBrush("AppAccentBrush",   "#1E88E5");
    }
}
