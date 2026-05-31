using ZooShop.ViewModels;

namespace ZooShop.Views.Pages;

public partial class StaffPage : System.Windows.Controls.Page
{
    public StaffPage(StaffViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        Loaded += async (_, _) => await vm.LoadStaffAsync();
    }
}
