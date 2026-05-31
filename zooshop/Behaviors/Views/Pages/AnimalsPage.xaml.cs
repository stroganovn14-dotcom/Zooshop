using Microsoft.Win32;
using System.Windows;
using ZooShop.ViewModels;

namespace ZooShop.Views.Pages;

public partial class AnimalsPage : System.Windows.Controls.Page
{
    private readonly AnimalsViewModel _vm;

    public AnimalsPage(AnimalsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        _vm = vm;
        Loaded += async (_, _) => await vm.LoadAsync();
    }

    private void BrowseImage_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title  = "Выберите изображение животного",
            Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp|Все файлы|*.*"
        };
        if (dlg.ShowDialog() == true)
            _vm.EditImagePath = dlg.FileName;
    }
}
