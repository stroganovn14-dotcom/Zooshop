using System.Windows;
using System.Windows.Controls;
using ZooShop.ViewModels;

namespace ZooShop.Views;

public partial class LoginWindow : Wpf.Ui.Controls.FluentWindow
{
    private readonly LoginViewModel _vm;

    public LoginWindow(LoginViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        _vm = vm;
        PwdBox.PasswordChanged += (_, _) => _vm.Password = PwdBox.Password;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1) DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
        => Application.Current.Shutdown();
}
