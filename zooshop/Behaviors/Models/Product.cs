using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZooShop.Models;

/// <summary>Товар на полке зоомагазина.</summary>
[Table("Products")]
public class Product
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    /// <summary>Остаток на складе (штук).</summary>
    public int Stock { get; set; }

    /// <summary>
    /// Срок годности. Если до истечения осталось < 7 дней —
    /// UI выделяет карточку красным индикатором.
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>Путь к локальному файлу изображения или URL.</summary>
    [MaxLength(500)]
    public string? ImagePath { get; set; }

    // ─── Вычислимые (NotMapped) свойства для UI ───────────────────────

    /// <summary>True, если товар заканчивается (< 7 дней до срока).</summary>
    [NotMapped]
    public bool IsExpiringSoon =>
        ExpirationDate.HasValue &&
        ExpirationDate.Value.Date <= DateTime.Today.AddDays(7);

    /// <summary>True, если товар уже просрочен.</summary>
    [NotMapped]
    public bool IsExpired =>
        ExpirationDate.HasValue &&
        ExpirationDate.Value.Date < DateTime.Today;
}
