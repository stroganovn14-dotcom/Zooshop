using ZooShop.ViewModels;

namespace ZooShop.Views.Pages;

public partial class SalesPage : System.Windows.Controls.Page
{
    public SalesPage(SalesViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        Loaded += async (_, _) => await vm.LoadAsync();
    }
}
