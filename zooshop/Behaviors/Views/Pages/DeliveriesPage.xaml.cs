using ZooShop.ViewModels;

namespace ZooShop.Views.Pages;

public partial class DeliveriesPage : System.Windows.Controls.Page
{
    public DeliveriesPage(DeliveriesViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        Loaded += async (_, _) => await vm.LoadAsync();
    }
}
