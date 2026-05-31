using System.Windows;

namespace ZooShop.Views;

public enum LogoutChoice { None, SwitchUser, ExitApp }

public partial class LogoutDialog : Window
{
    public LogoutChoice Choice { get; private set; } = LogoutChoice.None;

    public LogoutDialog() => InitializeComponent();

    private void SwitchUser_Click(object sender, RoutedEventArgs e)
    {
        Choice = LogoutChoice.SwitchUser;
        Close();
    }

    private void ExitApp_Click(object sender, RoutedEventArgs e)
    {
        Choice = LogoutChoice.ExitApp;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
}
