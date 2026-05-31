using System.Windows.Controls;
using ZooShop.ViewModels;

namespace ZooShop.Views.Pages;

public partial class ReportsPage : Page
{
    public ReportsPage(ReportsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
