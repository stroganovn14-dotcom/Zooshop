using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZooShop.Models;

[Table("DeliveryItems")]
public class DeliveryItem
{
    [Key] public int Id { get; set; }
    public int DeliveryId { get; set; }
    [ForeignKey(nameof(DeliveryId))] public Delivery Delivery { get; set; } = null!;
    public int ProductId { get; set; }
    [ForeignKey(nameof(ProductId))] public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal UnitCost { get; set; }
    [NotMapped] public decimal TotalCost => UnitCost * Quantity;
}
