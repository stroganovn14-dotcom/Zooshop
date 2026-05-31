namespace ZooShop.Models;

/// <summary>
/// Роли пользователей с числовым рангом.
/// Ранг используется в RBAC: Admin(3) > Manager(2) > Seller(1).
/// </summary>
public enum UserRole
{
    Seller  = 1,
    Manager = 2,
    Admin   = 3
}

/// <summary>Тип позиции в транзакции продажи.</summary>
public enum SaleItemType
{
    Product = 0,
    Animal  = 1
}
