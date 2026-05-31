namespace ZooShop.Services;

public interface IReportService
{
    Task ExportSalesAsync(string folder, DateTime? from, DateTime? to);
    Task ExportProductsAsync(string folder);
    Task ExportAnimalsAsync(string folder);
    Task ExportDeliveriesAsync(string folder, DateTime? from, DateTime? to);
}
