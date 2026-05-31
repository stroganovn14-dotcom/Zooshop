using Microsoft.EntityFrameworkCore;
using ZooShop.Data;
using ZooShop.Models;

namespace ZooShop.Services;

public sealed class AuthService : IAuthService
{
    private readonly IDbContextFactory<ZooShopDbContext> _dbFactory;

    public User? CurrentUser { get; private set; }

    public AuthService(IDbContextFactory<ZooShopDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<bool> LoginAsync(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            return false;

        await using var db = await _dbFactory.CreateDbContextAsync();

        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Login == login && u.PasswordHash == password);

        if (user is null) return false;

        CurrentUser = user;
        return true;
    }

    public void Logout() => CurrentUser = null;

    public bool CanEditUser(User target)
    {
        if (CurrentUser is null) return false;
        if (target.Id == CurrentUser.Id) return true;
        return target.Rank < CurrentUser.Rank;
    }
}
