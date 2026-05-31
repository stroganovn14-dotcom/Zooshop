using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZooShop.Models;

/// <summary>Животное, выставленное на продажу.</summary>
[Table("Animals")]
public class Animal
{
    [Key]
    public int Id { get; set; }

    /// <summary>Вид животного (кошка, собака, попугай…).</summary>
    [Required, MaxLength(100)]
    public string Species { get; set; } = string.Empty;

    /// <summary>Порода (может отсутствовать).</summary>
    [MaxLength(100)]
    public string? Breed { get; set; }

    /// <summary>Возраст в месяцах.</summary>
    public int AgeMonths { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    /// <summary>Описание состояния здоровья (напр. «Здорова, вакцинирована»).</summary>
    [Required, MaxLength(300)]
    public string HealthStatus { get; set; } = string.Empty;

    /// <summary>Путь к локальному файлу изображения или URL.</summary>
    [MaxLength(500)]
    public string? ImagePath { get; set; }

    // ─── Удобный вычислимый вид для UI ────────────────────────────────

    [NotMapped]
    public string DisplayName =>
        string.IsNullOrWhiteSpace(Breed) ? Species : $"{Species} ({Breed})";

    [NotMapped]
    public string AgeDisplay =>
        AgeMonths < 12
            ? $"{AgeMonths} мес."
            : $"{AgeMonths / 12} г. {AgeMonths % 12} мес.";
}
