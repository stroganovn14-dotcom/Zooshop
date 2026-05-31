using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ZooShop.Services;

namespace ZooShop.ViewModels;

public sealed partial class ReportsViewModel : ObservableObject
{
    private readonly IReportService _reports;

    public ReportsViewModel(IReportService reports)
    {
        _reports   = reports;
        FolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        SalesFrom  = DateTime.Today.AddMonths(-1);
        SalesTo    = DateTime.Today;
        DelFrom    = DateTime.Today.AddMonths(-1);
        DelTo      = DateTime.Today;
    }

    [ObservableProperty] private string   _folderPath = string.Empty;
    [ObservableProperty] private bool     _isBusy;
    [ObservableProperty] private string   _statusMessage = string.Empty;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(StatusSeverity))]
    private bool _hasError;

    public Wpf.Ui.Controls.InfoBarSeverity StatusSeverity =>
        HasError ? Wpf.Ui.Controls.InfoBarSeverity.Error : Wpf.Ui.Controls.InfoBarSeverity.Success;

    // Sales date range
    [ObservableProperty] private DateTime _salesFrom;
    [ObservableProperty] private DateTime _salesTo;

    // Deliveries date range
    [ObservableProperty] private DateTime _delFrom;
    [ObservableProperty] private DateTime _delTo;

    [RelayCommand]
    private void PickFolder()
    {
        var dlg = new OpenFolderDialog
        {
            Title            = "Выберите папку для сохранения отчётов",
            InitialDirectory = FolderPath,
        };
        if (dlg.ShowDialog() == true)
            FolderPath = dlg.FolderName;
    }

    [RelayCommand]
    private async Task ExportSalesAsync()
        => await RunExport(() => _reports.ExportSalesAsync(FolderPath, SalesFrom, SalesTo), "продажам");

    [RelayCommand]
    private async Task ExportProductsAsync()
        => await RunExport(() => _reports.ExportProductsAsync(FolderPath), "товарам");

    [RelayCommand]
    private async Task ExportAnimalsAsync()
        => await RunExport(() => _reports.ExportAnimalsAsync(FolderPath), "животным");

    [RelayCommand]
    private async Task ExportDeliveriesAsync()
        => await RunExport(() => _reports.ExportDeliveriesAsync(FolderPath, DelFrom, DelTo), "поставкам");

    private async Task RunExport(Func<Task> action, string reportName)
    {
        if (string.IsNullOrWhiteSpace(FolderPath)) return;
        IsBusy        = true;
        HasError      = false;
        StatusMessage = string.Empty;
        try
        {
            await action();
            StatusMessage = $"Отчёт по {reportName} сохранён в: {FolderPath}";
        }
        catch (Exception ex)
        {
            HasError      = true;
            StatusMessage = $"Ошибка: {ex.Message}";
        }
        finally { IsBusy = false; }
    }
}
