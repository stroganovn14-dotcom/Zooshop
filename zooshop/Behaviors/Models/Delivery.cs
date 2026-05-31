using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZooShop.Models;

[Table("Deliveries")]
public class Delivery
{
    [Key] public int Id { get; set; }
    public DateTime DeliveryDate { get; set; } = DateTime.UtcNow;
    [MaxLength(200)] public string? SupplierName { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal TotalCost { get; set; }
    public int AcceptedById { get; set; }
    [ForeignKey(nameof(AcceptedById))] public User AcceptedBy { get; set; } = null!;
    public ICollection<DeliveryItem> Items { get; set; } = new List<DeliveryItem>();

    [NotMapped] public string DateDisplay => DeliveryDate.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
    [NotMapped] public string SummaryDisplay => $"{(SupplierName ?? "—")} • {Items.Count} поз. • {TotalCost:N0} ₽";
}
