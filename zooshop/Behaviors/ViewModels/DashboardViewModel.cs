using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ZooShop.Data;

namespace ZooShop.ViewModels;

/// <summary>ViewModel главной (Dashboard) страницы.</summary>
public sealed partial class DashboardViewModel : ObservableObject
{
    private readonly IDbContextFactory<ZooShopDbContext> _dbFactory;

    public DashboardViewModel(IDbContextFactory<ZooShopDbContext> dbFactory)
        => _dbFactory = dbFactory;

    [ObservableProperty] private int _totalProducts;
    [ObservableProperty] private int _totalAnimals;
    [ObservableProperty] private int _todaySales;
    [ObservableProperty] private int _expiringCount;

    [RelayCommand]
    public async Task LoadAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        TotalProducts  = await db.Products.CountAsync();
        TotalAnimals   = await db.Animals.CountAsync();
        TodaySales     = await db.Sales
            .CountAsync(s => s.SaleDate.Date == DateTime.UtcNow.Date);
        ExpiringCount  = await db.Products
            .CountAsync(p => p.ExpirationDate != null &&
                             p.ExpirationDate.Value <= DateTime.Today.AddDays(7));
    }
}
