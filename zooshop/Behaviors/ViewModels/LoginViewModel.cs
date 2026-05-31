using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZooShop.Services;
using ZooShop.Views;

namespace ZooShop.ViewModels;

/// <summary>ViewModel окна авторизации.</summary>
public sealed partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly IServiceProvider _sp;

    public LoginViewModel(IAuthService auth, IServiceProvider sp)
    {
        _auth = auth;
        _sp   = sp;
    }

    [ObservableProperty] private string _login        = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool   _isBusy;

    /// <summary>
    /// Пароль — WPF-UI PasswordBox.Password является DependencyProperty,
    /// поэтому биндится напрямую через TwoWay.
    /// </summary>
    [ObservableProperty] private string _password = string.Empty;

    /// <summary>True при непустом ErrorMessage — управляет InfoBar.IsOpen.</summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    partial void OnErrorMessageChanged(string value)
        => OnPropertyChanged(nameof(HasError));

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Введите логин и пароль.";
            return;
        }

        IsBusy = true;
        try
        {
            var ok = await _auth.LoginAsync(Login.Trim(), Password);
            if (!ok)
            {
                ErrorMessage = "Неверный логин или пароль.";
                return;
            }

            // Открыть главное окно через DI
            var mainWindow = (MainWindow)_sp.GetService(typeof(MainWindow))!;
            mainWindow.Show();

            // Закрыть окно входа
            foreach (Window w in Application.Current.Windows)
            {
                if (w is LoginWindow lw) { lw.Close(); break; }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка подключения: {ex.Message}";
        }
        finally
        {
            IsBusy   = false;
            Password = string.Empty;
        }
    }
}
