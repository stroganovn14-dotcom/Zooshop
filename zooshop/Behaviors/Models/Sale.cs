using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZooShop.Models;

/// <summary>Запись о продаже товара или животного.</summary>
[Table("Sales")]
public class Sale
{
    [Key]
    public int Id { get; set; }

    /// <summary>Id связанного товара или животного (зависит от ItemType).</summary>
    public int ItemId { get; set; }

    /// <summary>Тип: Product или Animal.</summary>
    [Required]
    public SaleItemType ItemType { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalSum { get; set; }

    public DateTime SaleDate { get; set; } = DateTime.UtcNow;

    // ─── Продавец ─────────────────────────────────────────────────────

    public int SellerId { get; set; }

    [ForeignKey(nameof(SellerId))]
    public User Seller { get; set; } = null!;

    // ─── Данные о владельце (обязательны для животных) ────────────────

    /// <summary>ФИО нового владельца. Обязательно при продаже животного.</summary>
    [MaxLength(200)]
    public string? OwnerName { get; set; }

    /// <summary>
    /// Номер телефона владельца. Обязательно при продаже животного.
    /// Формат: +7XXXXXXXXXX или 8XXXXXXXXXX.
    /// </summary>
    [MaxLength(20)]
    public string? OwnerPhone { get; set; }

    // ─── Вычислимые свойства для UI ───────────────────────────────────

    [NotMapped]
    public string ItemTypeDisplay =>
        ItemType == SaleItemType.Animal ? "Животное" : "Товар";
}
