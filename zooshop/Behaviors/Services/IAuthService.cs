using ZooShop.Models;

namespace ZooShop.Services;

/// <summary>Контракт сервиса аутентификации.</summary>
public interface IAuthService
{
    /// <summary>Текущий вошедший пользователь. Null — не авторизован.</summary>
    User? CurrentUser { get; }

    /// <summary>Попытка входа. Возвращает true при успехе.</summary>
    Task<bool> LoginAsync(string login, string password);

    /// <summary>Завершить сессию.</summary>
    void Logout();

    /// <summary>
    /// Проверить, может ли CurrentUser редактировать/удалять target.
    /// Правило: TargetRank < CurrentRank ИЛИ это собственный профиль.
    /// </summary>
    bool CanEditUser(User target);
}
