using CommunityToolkit.Mvvm.ComponentModel;
using ZooShop.Models;

namespace ZooShop.ViewModels;

/// <summary>Позиция в корзине. Хранится только в памяти (не в БД).</summary>
public sealed partial class CartItem : ObservableObject
{
    public Product Product { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPrice))]
    private int _quantity;

    public decimal TotalPrice => Product.Price * Quantity;

    public CartItem(Product product, int quantity = 1)
    {
        Product   = product;
        _quantity = quantity;
    }
}
