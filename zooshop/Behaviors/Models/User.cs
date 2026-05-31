using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZooShop.Models;

/// <summary>Сотрудник зоомагазина. Ранг вычисляется из роли.</summary>
[Table("Users")]
public class User
{
    [Key]
    public int Id { get; set; }

    /// <summary>Полное имя (ФИО).</summary>
    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Уникальный логин для входа.</summary>
    [Required, MaxLength(100)]
    public string Login { get; set; } = string.Empty;

    /// <summary>BCrypt-хэш пароля. Никогда не хранить открытый пароль.</summary>
    [Required, MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Роль пользователя.</summary>
    [Required]
    public UserRole Role { get; set; }

    /// <summary>
    /// Числовой ранг для RBAC-сравнений.
    /// Computed property: Admin=3, Manager=2, Seller=1.
    /// </summary>
    [NotMapped]
    public int Rank => (int)Role;

    // Навигационное свойство — список продаж этого сотрудника
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
