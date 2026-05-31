using CommunityToolkit.Mvvm.ComponentModel;
using ZooShop.Models;

namespace ZooShop.ViewModels;

public sealed partial class DeliveryCartItem : ObservableObject
{
    public Product Product { get; }
    [ObservableProperty][NotifyPropertyChangedFor(nameof(TotalCost))] private int _quantity;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(TotalCost))] private decimal _unitCost;
    public decimal TotalCost => UnitCost * Quantity;

    public DeliveryCartItem(Product product, int quantity, decimal unitCost)
    {
        Product   = product;
        _quantity  = quantity;
        _unitCost  = unitCost;
    }
}
