using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ZooShop.Data;

namespace ZooShop.Services;

public sealed class ReportService : IReportService
{
    private readonly IDbContextFactory<ZooShopDbContext> _dbFactory;

    public ReportService(IDbContextFactory<ZooShopDbContext> dbFactory)
        => _dbFactory = dbFactory;

    public async Task ExportSalesAsync(string folder, DateTime? from, DateTime? to)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var q = db.Sales.Include(s => s.Seller).AsNoTracking();
        if (from.HasValue) q = q.Where(s => s.SaleDate >= from.Value);
        if (to.HasValue)   q = q.Where(s => s.SaleDate <= to.Value.AddDays(1));

        var list = await q.OrderByDescending(s => s.SaleDate).ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("ID;Дата;Тип;Продавец;Сумма (₽);Владелец;Телефон");
        foreach (var s in list)
            sb.AppendLine(
                $"{s.Id};" +
                $"{s.SaleDate.ToLocalTime():dd.MM.yyyy HH:mm};" +
                $"{s.ItemTypeDisplay};" +
                $"{Esc(s.Seller.FullName)};" +
                $"{s.TotalSum:N2};" +
                $"{Esc(s.OwnerName)};" +
                $"{Esc(s.OwnerPhone)}");

        await WriteAsync(folder, $"Продажи_{DateTime.Today:yyyy-MM-dd}.csv", sb);
    }

    public async Task ExportProductsAsync(string folder)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var list = await db.Products.AsNoTracking().OrderBy(p => p.Category).ThenBy(p => p.Name).ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("ID;Название;Категория;Цена (₽);Остаток;Срок годности;Истекает скоро;Просрочен");
        foreach (var p in list)
            sb.AppendLine(
                $"{p.Id};" +
                $"{Esc(p.Name)};" +
                $"{Esc(p.Category)};" +
                $"{p.Price:N2};" +
                $"{p.Stock};" +
                $"{(p.ExpirationDate.HasValue ? p.ExpirationDate.Value.ToString("dd.MM.yyyy") : "")};" +
                $"{(p.IsExpiringSoon ? "Да" : "Нет")};" +
                $"{(p.IsExpired ? "Да" : "Нет")}");

        await WriteAsync(folder, $"Товары_{DateTime.Today:yyyy-MM-dd}.csv", sb);
    }

    public async Task ExportAnimalsAsync(string folder)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var list = await db.Animals.AsNoTracking().OrderBy(a => a.Species).ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("ID;Вид;Порода;Возраст (мес.);Цена (₽);Состояние здоровья");
        foreach (var a in list)
            sb.AppendLine(
                $"{a.Id};" +
                $"{Esc(a.Species)};" +
                $"{Esc(a.Breed)};" +
                $"{a.AgeMonths};" +
                $"{a.Price:N2};" +
                $"{Esc(a.HealthStatus)}");

        await WriteAsync(folder, $"Животные_{DateTime.Today:yyyy-MM-dd}.csv", sb);
    }

    public async Task ExportDeliveriesAsync(string folder, DateTime? from, DateTime? to)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var q = db.Deliveries
            .Include(d => d.AcceptedBy)
            .Include(d => d.Items).ThenInclude(i => i.Product)
            .AsNoTracking();

        if (from.HasValue) q = q.Where(d => d.DeliveryDate >= from.Value);
        if (to.HasValue)   q = q.Where(d => d.DeliveryDate <= to.Value.AddDays(1));

        var list = await q.OrderByDescending(d => d.DeliveryDate).ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("ID поставки;Дата;Поставщик;Принял;Итого (₽);Товар;Количество;Цена за ед. (₽)");
        foreach (var d in list)
        {
            if (d.Items.Count == 0)
            {
                sb.AppendLine(
                    $"{d.Id};" +
                    $"{d.DeliveryDate.ToLocalTime():dd.MM.yyyy HH:mm};" +
                    $"{Esc(d.SupplierName)};" +
                    $"{Esc(d.AcceptedBy.FullName)};" +
                    $"{d.TotalCost:N2};;;");
            }
            else
            {
                foreach (var item in d.Items)
                    sb.AppendLine(
                        $"{d.Id};" +
                        $"{d.DeliveryDate.ToLocalTime():dd.MM.yyyy HH:mm};" +
                        $"{Esc(d.SupplierName)};" +
                        $"{Esc(d.AcceptedBy.FullName)};" +
                        $"{d.TotalCost:N2};" +
                        $"{Esc(item.Product.Name)};" +
                        $"{item.Quantity};" +
                        $"{item.UnitCost:N2}");
            }
        }

        await WriteAsync(folder, $"Поставки_{DateTime.Today:yyyy-MM-dd}.csv", sb);
    }

    // ── helpers ──────────────────────────────────────────────────────────

    private static string Esc(string? v)
    {
        if (string.IsNullOrEmpty(v)) return string.Empty;
        // wrap in quotes if contains semicolon or newline
        return v.Contains(';') || v.Contains('\n') ? $"\"{v.Replace("\"", "\"\"")}\"" : v;
    }

    private static async Task WriteAsync(string folder, string fileName, StringBuilder content)
    {
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, fileName);
        // UTF-8 BOM — Excel auto-detects encoding
        await File.WriteAllTextAsync(path, content.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }
}
