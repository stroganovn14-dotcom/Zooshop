using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ZooShop.Data;
using ZooShop.Models;
using ZooShop.Services;

namespace ZooShop.ViewModels;

/// <summary>
/// ViewModel страницы управления персоналом.
/// Все операции Update/Delete защищены RBAC-проверкой ранга.
/// </summary>
public sealed partial class StaffViewModel : ObservableObject
{
    private readonly IDbContextFactory<ZooShopDbContext> _dbFactory;
    private readonly IAuthService _auth;

    public StaffViewModel(
        IDbContextFactory<ZooShopDbContext> dbFactory,
        IAuthService auth)
    {
        _dbFactory = dbFactory;
        _auth      = auth;
    }

    // ─── Коллекции ────────────────────────────────────────────────────

    public ObservableCollection<User> Staff { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEditSelected))]
    [NotifyCanExecuteChangedFor(nameof(SaveUserCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteUserCommand))]
    private User? _selectedUser;

    // ─── Поля редактируемой формы ─────────────────────────────────────

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveUserCommand))]
    private string _editFullName  = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveUserCommand))]
    private string _editLogin     = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveUserCommand))]
    private string _editPassword  = string.Empty;

    [ObservableProperty] private UserRole _editRole    = UserRole.Seller;

    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool   _isBusy;

    /// <summary>True, если текущий пользователь может редактировать выбранного.</summary>
    public bool CanEditSelected =>
        SelectedUser is null || _auth.CanEditUser(SelectedUser);

    /// <summary>Список ролей для ComboBox. Admin видит все.</summary>
    public IEnumerable<UserRole> AvailableRoles =>
        Enum.GetValues<UserRole>()
            .Where(r => (int)r < (_auth.CurrentUser?.Rank ?? 0));

    // ─── Команды ──────────────────────────────────────────────────────

    /// <summary>Загрузить список сотрудников из БД.</summary>
    [RelayCommand]
    public async Task LoadStaffAsync()
    {
        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var users = await db.Users.AsNoTracking().ToListAsync();

            Staff.Clear();
            foreach (var u in users) Staff.Add(u);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Заполнить форму данными выбранного пользователя.</summary>
    partial void OnSelectedUserChanged(User? value)
    {
        if (value is null) return;
        EditFullName = value.FullName;
        EditLogin    = value.Login;
        EditPassword = string.Empty;
        EditRole     = value.Role;
        StatusMessage = string.Empty;
    }

    /// <summary>Создать нового пользователя (форма очищается).</summary>
    [RelayCommand]
    private void CreateNew()
    {
        SelectedUser  = null;
        EditFullName  = string.Empty;
        EditLogin     = string.Empty;
        EditPassword  = string.Empty;
        EditRole      = UserRole.Seller;
        StatusMessage = string.Empty;
    }

    /// <summary>Сохранить изменения / создать нового пользователя.</summary>
    [RelayCommand(CanExecute = nameof(CanSaveUser))]
    private async Task SaveUserAsync()
    {
        if (!ValidateForm()) return;

        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            if (SelectedUser is null)
            {
                // ─── Создание ─────────────────────────────────────────
                // Нельзя создать пользователя с рангом ≥ своего
                if ((int)EditRole >= (_auth.CurrentUser?.Rank ?? 0))
                {
                    StatusMessage = "Нельзя создать пользователя с рангом не ниже своего.";
                    return;
                }

                var newUser = new User
                {
                    FullName     = EditFullName.Trim(),
                    Login        = EditLogin.Trim(),
                    PasswordHash = EditPassword,
                    Role         = EditRole
                };
                db.Users.Add(newUser);
                await db.SaveChangesAsync();
                Staff.Add(newUser);
                StatusMessage = "Пользователь создан.";
            }
            else
            {
                // ─── Обновление ───────────────────────────────────────
                if (!_auth.CanEditUser(SelectedUser))
                {
                    StatusMessage = "Недостаточно прав для редактирования этого сотрудника.";
                    return;
                }

                var dbUser = await db.Users.FindAsync(SelectedUser.Id);
                if (dbUser is null) { StatusMessage = "Пользователь не найден."; return; }

                dbUser.FullName = EditFullName.Trim();
                dbUser.Login    = EditLogin.Trim();
                dbUser.Role     = EditRole;

                // Обновляем пароль только если введён новый
                if (!string.IsNullOrWhiteSpace(EditPassword))
                    dbUser.PasswordHash = EditPassword;

                await db.SaveChangesAsync();

                // Обновляем элемент в коллекции
                var idx = Staff.IndexOf(SelectedUser);
                if (idx >= 0) Staff[idx] = dbUser;
                SelectedUser = dbUser;
                StatusMessage = "Изменения сохранены.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка сохранения: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSaveUser() =>
        !string.IsNullOrWhiteSpace(EditFullName) &&
        !string.IsNullOrWhiteSpace(EditLogin) &&
        (SelectedUser is not null || !string.IsNullOrWhiteSpace(EditPassword));

    /// <summary>Удалить выбранного сотрудника.</summary>
    [RelayCommand(CanExecute = nameof(CanDeleteUser))]
    private async Task DeleteUserAsync()
    {
        if (SelectedUser is null) return;

        // RBAC-проверка
        if (!_auth.CanEditUser(SelectedUser))
        {
            StatusMessage = "Недостаточно прав для удаления этого сотрудника.";
            return;
        }

        // Нельзя удалить самого себя
        if (SelectedUser.Id == _auth.CurrentUser?.Id)
        {
            StatusMessage = "Нельзя удалить собственную учётную запись.";
            return;
        }

        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var dbUser = await db.Users.FindAsync(SelectedUser.Id);
            if (dbUser is null) { StatusMessage = "Пользователь не найден."; return; }

            db.Users.Remove(dbUser);
            await db.SaveChangesAsync();

            Staff.Remove(SelectedUser);
            SelectedUser  = null;
            StatusMessage = "Пользователь удалён.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка удаления: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanDeleteUser() =>
        SelectedUser is not null &&
        SelectedUser.Id != _auth.CurrentUser?.Id &&
        _auth.CanEditUser(SelectedUser!);

    // ─── Вспомогательные методы ───────────────────────────────────────

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(EditFullName))
        { StatusMessage = "Введите ФИО."; return false; }

        if (string.IsNullOrWhiteSpace(EditLogin))
        { StatusMessage = "Введите логин."; return false; }

        if (SelectedUser is null && string.IsNullOrWhiteSpace(EditPassword))
        { StatusMessage = "Введите пароль для нового пользователя."; return false; }

        return true;
    }
}
