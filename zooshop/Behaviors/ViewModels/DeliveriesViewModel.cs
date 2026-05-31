using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ZooShop.Data;
using ZooShop.Models;
using ZooShop.Services;

namespace ZooShop.ViewModels;

public sealed partial class DeliveriesViewModel : ObservableObject
{
    private readonly IDbContextFactory<ZooShopDbContext> _dbFactory;
    private readonly IAuthService _auth;

    public DeliveriesViewModel(IDbContextFactory<ZooShopDbContext> dbFactory, IAuthService auth)
    {
        _dbFactory = dbFactory;
        _auth      = auth;
        Cart.CollectionChanged += (_, _) => RefreshCartTotals();
    }

    public bool CanAccept => _auth.CurrentUser?.Rank >= (int)UserRole.Manager;

    public ObservableCollection<Delivery>        Deliveries { get; } = new();
    public ObservableCollection<Product>          Products   { get; } = new();
    public ObservableCollection<DeliveryCartItem> Cart       { get; } = new();

    [ObservableProperty] private bool     _isBusy;
    [ObservableProperty] private string   _supplierName     = string.Empty;
    [ObservableProperty] private Product? _selectedProduct;
    [ObservableProperty] private int      _deliveryQuantity = 1;
    [ObservableProperty] private decimal  _deliveryUnitCost;
    [ObservableProperty] private string   _addError         = string.Empty;
    [ObservableProperty] private string   _confirmError     = string.Empty;

    public decimal CartTotal      => Cart.Sum(i => i.TotalCost);
    public bool    IsCartEmpty    => Cart.Count == 0;
    public bool    IsCartNotEmpty => Cart.Count > 0;

    partial void OnSelectedProductChanged(Product? value)
    {
        if (value is not null) DeliveryUnitCost = value.Price;
    }

    private void RefreshCartTotals()
    {
        OnPropertyChanged(nameof(CartTotal));
        OnPropertyChanged(nameof(IsCartEmpty));
        OnPropertyChanged(nameof(IsCartNotEmpty));
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var deliveries = await db.Deliveries
                .AsNoTracking()
                .Include(d => d.AcceptedBy)
                .Include(d => d.Items).ThenInclude(i => i.Product)
                .OrderByDescending(d => d.DeliveryDate)
                .ToListAsync();
            Deliveries.Clear();
            foreach (var d in deliveries) Deliveries.Add(d);

            var products = await db.Products.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
            Products.Clear();
            foreach (var p in products) Products.Add(p);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void AddToDelivery()
    {
        AddError = string.Empty;
        if (SelectedProduct is null) { AddError = "Выберите товар."; return; }
        if (DeliveryQuantity <= 0)   { AddError = "Количество должно быть больше нуля."; return; }
        if (DeliveryUnitCost < 0)    { AddError = "Цена не может быть отрицательной."; return; }

        var existing = Cart.FirstOrDefault(i => i.Product.Id == SelectedProduct.Id);
        if (existing is not null)
            existing.Quantity += DeliveryQuantity;
        else
            Cart.Add(new DeliveryCartItem(SelectedProduct, DeliveryQuantity, DeliveryUnitCost));

        RefreshCartTotals();
        DeliveryQuantity = 1;
    }

    [RelayCommand]
    private void RemoveFromDelivery(DeliveryCartItem item) => Cart.Remove(item);

    [RelayCommand]
    private void ClearCart()
    {
        Cart.Clear();
        SupplierName = string.Empty;
        AddError     = string.Empty;
        ConfirmError = string.Empty;
    }

    [RelayCommand]
    private async Task ConfirmDeliveryAsync()
    {
        if (Cart.Count == 0) return;
        ConfirmError = string.Empty;
        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var delivery = new Delivery
            {
                DeliveryDate = DateTime.UtcNow,
                SupplierName = string.IsNullOrWhiteSpace(SupplierName) ? null : SupplierName.Trim(),
                TotalCost    = CartTotal,
                AcceptedById = _auth.CurrentUser!.Id
            };

            foreach (var item in Cart)
            {
                var dbProd = await db.Products.FindAsync(item.Product.Id);
                if (dbProd is null) continue;
                dbProd.Stock += item.Quantity;
                delivery.Items.Add(new DeliveryItem
                {
                    ProductId = item.Product.Id,
                    Quantity  = item.Quantity,
                    UnitCost  = item.UnitCost
                });
            }

            db.Deliveries.Add(delivery);
            await db.SaveChangesAsync();
            Cart.Clear();
            SupplierName = string.Empty;
            await LoadAsync();
        }
        catch (Exception ex) { ConfirmError = $"Ошибка: {ex.Message}"; }
        finally { IsBusy = false; }
    }
}
