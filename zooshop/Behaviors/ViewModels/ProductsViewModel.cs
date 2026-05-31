using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ZooShop.Data;
using ZooShop.Models;
using ZooShop.Services;

namespace ZooShop.ViewModels;

public sealed partial class ProductsViewModel : ObservableObject
{
    private readonly IDbContextFactory<ZooShopDbContext> _dbFactory;
    private readonly IAuthService _auth;

    public ProductsViewModel(
        IDbContextFactory<ZooShopDbContext> dbFactory,
        IAuthService auth)
    {
        _dbFactory = dbFactory;
        _auth      = auth;
        Cart.CollectionChanged += (_, _) => RefreshCartTotals();
    }

    public bool CanManage => _auth.CurrentUser?.Rank >= (int)UserRole.Manager;
    public bool CanDelete => _auth.CurrentUser?.Rank >= (int)UserRole.Admin;

    public ObservableCollection<Product> Products   { get; } = new();
    public ObservableCollection<string>  Categories { get; } = new();

    [ObservableProperty] private bool   _isBusy;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public ObservableCollection<CartItem> Cart { get; } = new();

    public decimal CartTotal      => Cart.Sum(i => i.TotalPrice);
    public int     CartCount      => Cart.Sum(i => i.Quantity);
    public bool    IsCartEmpty    => Cart.Count == 0;
    public bool    IsCartNotEmpty => Cart.Count > 0;

    [ObservableProperty] private string _cartError = string.Empty;

    private void RefreshCartTotals()
    {
        OnPropertyChanged(nameof(CartTotal));
        OnPropertyChanged(nameof(CartCount));
        OnPropertyChanged(nameof(IsCartEmpty));
        OnPropertyChanged(nameof(IsCartNotEmpty));
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditFormVisible))]
    [NotifyPropertyChangedFor(nameof(IsCartVisible))]
    private bool _isEditMode;

    public bool IsEditFormVisible => IsEditMode;
    public bool IsCartVisible     => !IsEditMode;

    [ObservableProperty] private Product?  _editingProduct;
    [ObservableProperty] private string    _editName        = string.Empty;
    [ObservableProperty] private string    _editCategory    = string.Empty;
    [ObservableProperty] private decimal   _editPrice;
    [ObservableProperty] private int       _editStock;
    [ObservableProperty] private DateTime? _editExpiration;
    [ObservableProperty] private string    _editImagePath   = string.Empty;
    [ObservableProperty] private string    _editError       = string.Empty;

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var list = await db.Products.AsNoTracking().ToListAsync();
            Products.Clear();
            foreach (var p in list) Products.Add(p);
            var cats = list.Select(p => p.Category).Distinct().OrderBy(c => c);
            Categories.Clear();
            foreach (var c in cats) Categories.Add(c);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void AddToCart(Product product)
    {
        if (product.Stock <= 0) return;
        var existing = Cart.FirstOrDefault(i => i.Product.Id == product.Id);
        if (existing is not null)
        {
            if (existing.Quantity < product.Stock)
            {
                existing.Quantity++;
                RefreshCartTotals();
            }
        }
        else
        {
            Cart.Add(new CartItem(product, 1));
        }
    }

    [RelayCommand]
    private void RemoveFromCart(CartItem item) => Cart.Remove(item);

    [RelayCommand]
    private void IncrementItem(CartItem item)
    {
        if (item.Quantity < item.Product.Stock)
        {
            item.Quantity++;
            RefreshCartTotals();
        }
    }

    [RelayCommand]
    private void DecrementItem(CartItem item)
    {
        if (item.Quantity > 1)
        {
            item.Quantity--;
            RefreshCartTotals();
        }
        else
        {
            Cart.Remove(item);
        }
    }

    [RelayCommand]
    private void ClearCart()
    {
        Cart.Clear();
        CartError = string.Empty;
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (Cart.Count == 0) return;
        CartError = string.Empty;
        IsBusy    = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            foreach (var item in Cart)
            {
                var dbProd = await db.Products.FindAsync(item.Product.Id);
                if (dbProd is null || item.Quantity > dbProd.Stock)
                {
                    CartError = $"Недостаточно «{item.Product.Name}» на складе.";
                    return;
                }
            }
            foreach (var item in Cart)
            {
                var dbProd = await db.Products.FindAsync(item.Product.Id);
                dbProd!.Stock -= item.Quantity;
                db.Sales.Add(new Sale
                {
                    ItemId   = item.Product.Id,
                    ItemType = SaleItemType.Product,
                    TotalSum = item.TotalPrice,
                    SaleDate = DateTime.UtcNow,
                    SellerId = _auth.CurrentUser!.Id
                });
            }
            await db.SaveChangesAsync();
            Cart.Clear();
            CartError     = string.Empty;
            StatusMessage = "Продажа оформлена.";
            await LoadAsync();
        }
        catch (Exception ex) { CartError = $"Ошибка: {ex.Message}"; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void OpenEdit(Product? product)
    {
        EditingProduct = product;
        EditName       = product?.Name        ?? string.Empty;
        EditCategory   = product?.Category    ?? string.Empty;
        EditPrice      = product?.Price       ?? 0;
        EditStock      = product?.Stock       ?? 0;
        EditExpiration = product?.ExpirationDate;
        EditImagePath  = product?.ImagePath   ?? string.Empty;
        EditError      = string.Empty;
        IsEditMode     = true;
    }

    [RelayCommand]
    private void CancelEdit() => IsEditMode = false;

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditName))
        { EditError = "Введите название товара."; return; }
        if (EditPrice < 0)
        { EditError = "Цена не может быть отрицательной."; return; }

        IsBusy    = true;
        EditError = string.Empty;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            if (EditingProduct is null)
            {
                var p = new Product
                {
                    Name           = EditName.Trim(),
                    Category       = EditCategory.Trim(),
                    Price          = EditPrice,
                    Stock          = EditStock,
                    ExpirationDate = EditExpiration,
                    ImagePath      = string.IsNullOrWhiteSpace(EditImagePath) ? null : EditImagePath.Trim()
                };
                db.Products.Add(p);
                await db.SaveChangesAsync();
                Products.Add(p);
                StatusMessage = "Товар добавлен.";
                IsEditMode    = false;
            }
            else
            {
                var dbProd = await db.Products.FindAsync(EditingProduct.Id);
                if (dbProd is null) return;
                dbProd.Name           = EditName.Trim();
                dbProd.Category       = EditCategory.Trim();
                dbProd.Price          = EditPrice;
                dbProd.Stock          = EditStock;
                dbProd.ExpirationDate = EditExpiration;
                dbProd.ImagePath      = string.IsNullOrWhiteSpace(EditImagePath) ? null : EditImagePath.Trim();
                await db.SaveChangesAsync();
                var idx = Products.IndexOf(EditingProduct);
                if (idx >= 0) Products[idx] = dbProd;
                StatusMessage = "Товар обновлён.";
                IsEditMode    = false;
            }
        }
        catch (Exception ex) { EditError = $"Ошибка: {ex.Message}"; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (EditingProduct is null) return;
        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var dbProd = await db.Products.FindAsync(EditingProduct.Id);
            if (dbProd is not null) db.Products.Remove(dbProd);
            await db.SaveChangesAsync();
            Products.Remove(EditingProduct);
            EditingProduct = null;
            IsEditMode     = false;
            StatusMessage  = "Товар удалён.";
        }
        catch (Exception ex) { EditError = $"Ошибка: {ex.Message}"; }
        finally { IsBusy = false; }
    }
}
