using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ZooShop.Data;
using ZooShop.Models;

namespace ZooShop.ViewModels;

/// <summary>ViewModel страницы журнала продаж.</summary>
public sealed partial class SalesViewModel : ObservableObject
{
    private readonly IDbContextFactory<ZooShopDbContext> _dbFactory;

    public SalesViewModel(IDbContextFactory<ZooShopDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
        FilterFrom = DateTime.Today.AddMonths(-1);
        FilterTo   = DateTime.Today;
    }

    public ObservableCollection<Sale> Sales { get; } = new();

    [ObservableProperty] private DateTime? _filterFrom;
    [ObservableProperty] private DateTime? _filterTo;
    [ObservableProperty] private bool      _isBusy;

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var query = db.Sales
                .Include(s => s.Seller)
                .AsNoTracking();

            if (FilterFrom.HasValue)
                query = query.Where(s => s.SaleDate >= FilterFrom.Value);

            if (FilterTo.HasValue)
                query = query.Where(s => s.SaleDate <= FilterTo.Value.AddDays(1));

            var list = await query
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();

            Sales.Clear();
            foreach (var s in list) Sales.Add(s);
        }
        finally { IsBusy = false; }
    }
}
