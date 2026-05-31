using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ZooShop.Data;
using ZooShop.Models;
using ZooShop.Services;

namespace ZooShop.ViewModels;

public sealed partial class AnimalsViewModel : ObservableObject
{
    private readonly IDbContextFactory<ZooShopDbContext> _dbFactory;
    private readonly IAuthService _auth;

    public AnimalsViewModel(
        IDbContextFactory<ZooShopDbContext> dbFactory,
        IAuthService auth)
    {
        _dbFactory = dbFactory;
        _auth      = auth;
    }

    public ObservableCollection<Animal> Animals { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSaleFormVisible))]
    private Animal? _selectedAnimal;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSaleFormVisible))]
    private bool _isSaleMode;

    [ObservableProperty] private string _ownerName  = string.Empty;
    [ObservableProperty] private string _ownerPhone = string.Empty;
    [ObservableProperty] private string _saleError  = string.Empty;

    public bool IsSaleFormVisible => IsSaleMode && SelectedAnimal is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAddFormVisible))]
    [NotifyPropertyChangedFor(nameof(FormTitle))]
    [NotifyPropertyChangedFor(nameof(CanDeleteAnimal))]
    private bool _isAddMode;

    public bool IsAddFormVisible => IsAddMode;

    // null = новое животное, not null = редактирование
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormTitle))]
    [NotifyPropertyChangedFor(nameof(CanDeleteAnimal))]
    private Animal? _editingAnimal;

    public string FormTitle    => EditingAnimal is null ? "Новое животное" : "Редактировать животное";
    public bool   CanDeleteAnimal => EditingAnimal is not null && _auth.CurrentUser?.Rank >= (int)UserRole.Admin;

    [ObservableProperty] private string  _editSpecies      = string.Empty;
    [ObservableProperty] private string  _editBreed        = string.Empty;
    [ObservableProperty] private int     _editAgeMonths    = 1;
    [ObservableProperty] private decimal _editPrice;
    [ObservableProperty] private string  _editHealthStatus = string.Empty;
    [ObservableProperty] private string  _editImagePath    = string.Empty;
    [ObservableProperty] private string  _addError         = string.Empty;

    public bool CanManage => _auth.CurrentUser?.Rank >= (int)UserRole.Manager;
    public bool CanDelete => _auth.CurrentUser?.Rank >= (int)UserRole.Admin;

    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool   _isBusy;

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var list = await db.Animals.AsNoTracking().ToListAsync();
            Animals.Clear();
            foreach (var a in list) Animals.Add(a);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void StartSale(Animal animal)
    {
        IsAddMode      = false;
        SelectedAnimal = animal;
        OwnerName      = string.Empty;
        OwnerPhone     = string.Empty;
        SaleError      = string.Empty;
        IsSaleMode     = true;
    }

    [RelayCommand]
    private void CancelSale()
    {
        IsSaleMode     = false;
        SelectedAnimal = null;
    }

    [RelayCommand]
    private async Task ConfirmSaleAsync()
    {
        SaleError = string.Empty;

        if (string.IsNullOrWhiteSpace(OwnerName))
        { SaleError = "Введите ФИО владельца."; return; }

        var phoneDigits = new string(OwnerPhone.Where(char.IsDigit).ToArray());
        if (phoneDigits.Length > 0 && (phoneDigits[0] == '7' || phoneDigits[0] == '8'))
            phoneDigits = phoneDigits[1..];
        if (phoneDigits.Length != 10)
        { SaleError = "Введите корректный номер телефона (10 цифр)."; return; }

        if (SelectedAnimal is null) return;

        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var rawDigits = new string(OwnerPhone.Where(char.IsDigit).ToArray());
            if (rawDigits.Length > 0 && (rawDigits[0] == '7' || rawDigits[0] == '8'))
                rawDigits = rawDigits[1..];

            db.Sales.Add(new Sale
            {
                ItemId     = SelectedAnimal.Id,
                ItemType   = SaleItemType.Animal,
                TotalSum   = SelectedAnimal.Price,
                SaleDate   = DateTime.UtcNow,
                SellerId   = _auth.CurrentUser!.Id,
                OwnerName  = OwnerName.Trim(),
                OwnerPhone = $"+7{rawDigits}"
            });

            var dbAnimal = await db.Animals.FindAsync(SelectedAnimal.Id);
            if (dbAnimal is not null) db.Animals.Remove(dbAnimal);

            await db.SaveChangesAsync();

            Animals.Remove(SelectedAnimal);
            SelectedAnimal = null;
            IsSaleMode     = false;
            StatusMessage  = "Животное успешно продано.";
        }
        catch (Exception ex) { SaleError = $"Ошибка: {ex.Message}"; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void OpenAddForm()
    {
        IsSaleMode       = false;
        SelectedAnimal   = null;
        EditingAnimal    = null;
        EditSpecies      = string.Empty;
        EditBreed        = string.Empty;
        EditAgeMonths    = 1;
        EditPrice        = 0;
        EditHealthStatus = string.Empty;
        EditImagePath    = string.Empty;
        AddError         = string.Empty;
        IsAddMode        = true;
    }

    [RelayCommand]
    private void OpenEditForm(Animal animal)
    {
        IsSaleMode       = false;
        SelectedAnimal   = null;
        EditingAnimal    = animal;
        EditSpecies      = animal.Species;
        EditBreed        = animal.Breed ?? string.Empty;
        EditAgeMonths    = animal.AgeMonths;
        EditPrice        = animal.Price;
        EditHealthStatus = animal.HealthStatus;
        EditImagePath    = animal.ImagePath ?? string.Empty;
        AddError         = string.Empty;
        IsAddMode        = true;
    }

    [RelayCommand]
    private void CancelAdd()
    {
        IsAddMode     = false;
        EditingAnimal = null;
    }

    [RelayCommand]
    private async Task SaveAnimalAsync()
    {
        AddError = string.Empty;

        if (string.IsNullOrWhiteSpace(EditSpecies))
        { AddError = "Введите вид животного."; return; }
        if (EditPrice <= 0)
        { AddError = "Введите цену больше нуля."; return; }
        if (EditAgeMonths < 0)
        { AddError = "Возраст не может быть отрицательным."; return; }

        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            if (EditingAnimal is null)
            {
                db.Animals.Add(new Animal
                {
                    Species      = EditSpecies.Trim(),
                    Breed        = string.IsNullOrWhiteSpace(EditBreed) ? null : EditBreed.Trim(),
                    AgeMonths    = EditAgeMonths,
                    Price        = EditPrice,
                    HealthStatus = string.IsNullOrWhiteSpace(EditHealthStatus) ? "Здоров" : EditHealthStatus.Trim(),
                    ImagePath    = string.IsNullOrWhiteSpace(EditImagePath) ? null : EditImagePath.Trim()
                });
                StatusMessage = "Животное добавлено.";
            }
            else
            {
                var dbAnimal = await db.Animals.FindAsync(EditingAnimal.Id);
                if (dbAnimal is null) return;
                dbAnimal.Species      = EditSpecies.Trim();
                dbAnimal.Breed        = string.IsNullOrWhiteSpace(EditBreed) ? null : EditBreed.Trim();
                dbAnimal.AgeMonths    = EditAgeMonths;
                dbAnimal.Price        = EditPrice;
                dbAnimal.HealthStatus = string.IsNullOrWhiteSpace(EditHealthStatus) ? "Здоров" : EditHealthStatus.Trim();
                dbAnimal.ImagePath    = string.IsNullOrWhiteSpace(EditImagePath) ? null : EditImagePath.Trim();
                StatusMessage = "Животное обновлено.";
            }

            await db.SaveChangesAsync();
            await LoadAsync();
            IsAddMode     = false;
            EditingAnimal = null;
        }
        catch (Exception ex) { AddError = $"Ошибка: {ex.Message}"; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task DeleteAnimalAsync()
    {
        if (EditingAnimal is null) return;
        IsBusy = true;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var dbAnimal = await db.Animals.FindAsync(EditingAnimal.Id);
            if (dbAnimal is not null) db.Animals.Remove(dbAnimal);
            await db.SaveChangesAsync();
            Animals.Remove(EditingAnimal);
            EditingAnimal = null;
            IsAddMode     = false;
            StatusMessage = "Животное удалено.";
        }
        catch (Exception ex) { AddError = $"Ошибка: {ex.Message}"; }
        finally { IsBusy = false; }
    }
}
