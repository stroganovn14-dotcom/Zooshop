using ZooShop.ViewModels;

namespace ZooShop.Views.Pages;

public partial class DashboardPage : System.Windows.Controls.Page
{
    public DashboardPage(DashboardViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        Loaded += async (_, _) => await vm.LoadAsync();
    }
}
