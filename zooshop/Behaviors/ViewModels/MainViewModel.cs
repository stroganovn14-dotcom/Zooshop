using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZooShop.Models;
using ZooShop.Services;

namespace ZooShop.ViewModels;

/// <summary>ViewModel главного окна.</summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly INavigationService _nav;
    private readonly IServiceProvider _sp;

    public MainViewModel(IAuthService auth, INavigationService nav, IServiceProvider sp)
    {
        _auth = auth;
        _nav  = nav;
        _sp   = sp;
    }

    /// <summary>Текущий авторизованный пользователь (для отображения в шапке).</summary>
    public User? CurrentUser => _auth.CurrentUser;

    /// <summary>Показывать ли пункт «Персонал» (только Manager и выше).</summary>
    public bool CanViewStaff =>
        _auth.CurrentUser?.Rank >= (int)UserRole.Manager;

    [RelayCommand]
    private void Logout()
    {
        _auth.Logout();

        // Открыть окно входа через DI и закрыть главное
        var loginWin = (Views.LoginWindow)_sp.GetService(typeof(Views.LoginWindow))!;
        loginWin.Show();

        foreach (System.Windows.Window w in System.Windows.Application.Current.Windows)
        {
            if (w is Views.MainWindow main)
            {
                main.Close();
                break;
            }
        }
    }
}
