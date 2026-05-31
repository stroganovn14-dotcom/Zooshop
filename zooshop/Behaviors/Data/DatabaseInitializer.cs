using Microsoft.EntityFrameworkCore;
using ZooShop.Models;

namespace ZooShop.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(ZooShopDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        await db.Database.ExecuteSqlRawAsync(@"
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Deliveries')
BEGIN
    CREATE TABLE Deliveries (
        Id           int IDENTITY(1,1) NOT NULL,
        DeliveryDate datetime2(7)      NOT NULL,
        SupplierName nvarchar(200)     NULL,
        TotalCost    decimal(18,2)     NOT NULL,
        AcceptedById int               NOT NULL,
        CONSTRAINT PK_Deliveries        PRIMARY KEY (Id),
        CONSTRAINT FK_Deliveries_Users  FOREIGN KEY (AcceptedById) REFERENCES Users(Id)
    );
    CREATE TABLE DeliveryItems (
        Id         int IDENTITY(1,1) NOT NULL,
        DeliveryId int               NOT NULL,
        ProductId  int               NOT NULL,
        Quantity   int               NOT NULL,
        UnitCost   decimal(18,2)     NOT NULL,
        CONSTRAINT PK_DeliveryItems          PRIMARY KEY (Id),
        CONSTRAINT FK_DeliveryItems_Delivery FOREIGN KEY (DeliveryId) REFERENCES Deliveries(Id) ON DELETE CASCADE,
        CONSTRAINT FK_DeliveryItems_Product  FOREIGN KEY (ProductId)  REFERENCES Products(Id)   ON DELETE CASCADE
    );
END");

        // Заполняем только если таблицы пустые (первый запуск)
        if (!db.Users.Any())    await SeedUsersAsync(db);
        if (!db.Products.Any()) await SeedProductsAsync(db);
        if (!db.Animals.Any())  await SeedAnimalsAsync(db);

        // Проставляем картинки если ещё не заданы
        await PatchImagePathsAsync(db);
    }

    private static async Task PatchImagePathsAsync(ZooShopDbContext db)
    {
        // N-префикс обязателен для корректного сравнения кириллицы в nvarchar-столбцах
        // независимо от collation сервера. Условие покрывает и NULL, и пустую строку.
        await db.Database.ExecuteSqlRawAsync(@"
UPDATE Products SET ImagePath = N'Images\royal-canin.png'       WHERE (ImagePath IS NULL OR ImagePath = N'') AND Name LIKE N'%Royal Canin%';
UPDATE Products SET ImagePath = N'Images\purina-proplan.webp'   WHERE (ImagePath IS NULL OR ImagePath = N'') AND Name LIKE N'%Purina%';
UPDATE Products SET ImagePath = N'Images\vitamins-grizuni.webp' WHERE (ImagePath IS NULL OR ImagePath = N'') AND Name LIKE N'%Витамин%';
UPDATE Products SET ImagePath = N'Images\dreamis.webp'          WHERE (ImagePath IS NULL OR ImagePath = N'') AND Name LIKE N'%Dreamies%';
UPDATE Products SET ImagePath = N'Images\mish.webp'             WHERE (ImagePath IS NULL OR ImagePath = N'') AND Name LIKE N'%Игрушка%';
UPDATE Products SET ImagePath = N'Images\aquarium.webp'         WHERE (ImagePath IS NULL OR ImagePath = N'') AND Name LIKE N'%Аквариум%';
UPDATE Products SET ImagePath = N'Images\lezhanka.webp'         WHERE (ImagePath IS NULL OR ImagePath = N'') AND Name LIKE N'%Лежанка%';
UPDATE Products SET ImagePath = N'Images\napolnitel.webp'       WHERE (ImagePath IS NULL OR ImagePath = N'') AND Name LIKE N'%Наполнитель%';

UPDATE Animals SET ImagePath = N'Images\shotlandec.webp'        WHERE (ImagePath IS NULL OR ImagePath = N'') AND Breed LIKE N'%Шотланд%';
UPDATE Animals SET ImagePath = N'Images\retriver.webp'          WHERE (ImagePath IS NULL OR ImagePath = N'') AND Breed LIKE N'%Лабрадор%';
UPDATE Animals SET ImagePath = N'Images\popugaivolnistiy.jpg'   WHERE (ImagePath IS NULL OR ImagePath = N'') AND Breed LIKE N'%Волнист%';
UPDATE Animals SET ImagePath = N'Images\karlikoviybaran.jpg'    WHERE (ImagePath IS NULL OR ImagePath = N'') AND Breed LIKE N'%баран%';
UPDATE Animals SET ImagePath = N'Images\dzhungarskiy.jpg'       WHERE (ImagePath IS NULL OR ImagePath = N'') AND Breed LIKE N'%Джунгар%';
UPDATE Animals SET ImagePath = N'Images\mainkun.jpg'            WHERE (ImagePath IS NULL OR ImagePath = N'') AND Breed LIKE N'%кун%';
UPDATE Animals SET ImagePath = N'Images\i.webp'                 WHERE (ImagePath IS NULL OR ImagePath = N'') AND Breed LIKE N'%Хаски%';
");
    }

    private static async Task SeedUsersAsync(ZooShopDbContext db)
    {
        db.Users.AddRange(
            new User { FullName = "Главный Администратор",   Login = "admin",    PasswordHash = "admin",    Role = UserRole.Admin   },
            new User { FullName = "Иванов Иван Иванович",    Login = "manager1", PasswordHash = "manager1", Role = UserRole.Manager },
            new User { FullName = "Петрова Мария Сергеевна", Login = "seller1",  PasswordHash = "seller1",  Role = UserRole.Seller  }
        );
        await db.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(ZooShopDbContext db)
    {
        db.Products.AddRange(
            new Product { Name = "Корм Royal Canin для кошек 2кг",  Category = "Корма",          Price = 1299, Stock = 50, ExpirationDate = DateTime.Today.AddDays(3),   ImagePath = @"Images\royal-canin.png"       },
            new Product { Name = "Корм Purina Pro Plan для собак",   Category = "Корма",          Price =  899, Stock = 30, ExpirationDate = DateTime.Today.AddDays(180), ImagePath = @"Images\purina-proplan.webp"   },
            new Product { Name = "Витамины для грызунов Beaphar",    Category = "Витамины",       Price =  450, Stock = 20, ExpirationDate = DateTime.Today.AddDays(5),   ImagePath = @"Images\vitamins-grizuni.webp" },
            new Product { Name = "Лакомства Dreamies для кошек",     Category = "Лакомства",      Price =  199, Stock = 15, ExpirationDate = DateTime.Today.AddDays(-2),  ImagePath = @"Images\dreamis.webp"          },
            new Product { Name = "Игрушка-мышь для кошки",           Category = "Игрушки",        Price =  299, Stock = 100,                                               ImagePath = @"Images\mish.webp"             },
            new Product { Name = "Аквариум 50л с крышкой",           Category = "Аквариумистика", Price = 3500, Stock =   5,                                               ImagePath = @"Images\aquarium.webp"         },
            new Product { Name = "Лежанка ортопедическая 70×50",     Category = "Аксессуары",     Price = 1800, Stock =  15,                                               ImagePath = @"Images\lezhanka.webp"         },
            new Product { Name = "Наполнитель силикагелевый 5л",     Category = "Гигиена",        Price =  580, Stock =  60, ExpirationDate = DateTime.Today.AddDays(365), ImagePath = @"Images\napolnitel.webp"       }
        );
        await db.SaveChangesAsync();
    }

    private static async Task SeedAnimalsAsync(ZooShopDbContext db)
    {
        db.Animals.AddRange(
            new Animal { Species = "Кошка",   Breed = "Шотландская вислоухая", AgeMonths =  3, Price = 25000, HealthStatus = "Здорова, вакцинирована", ImagePath = @"Images\shotlandec.webp"      },
            new Animal { Species = "Собака",  Breed = "Лабрадор-ретривер",     AgeMonths =  6, Price = 45000, HealthStatus = "Здоров, вакцинирован",   ImagePath = @"Images\retriver.webp"        },
            new Animal { Species = "Попугай", Breed = "Волнистый",             AgeMonths = 12, Price =  3500, HealthStatus = "Здоров, ручной",          ImagePath = @"Images\popugaivolnistiy.jpg" },
            new Animal { Species = "Кролик",  Breed = "Карликовый баран",      AgeMonths =  2, Price =  5000, HealthStatus = "Здоров, вакцинирован",    ImagePath = @"Images\karlikoviybaran.jpg"  },
            new Animal { Species = "Хомяк",   Breed = "Джунгарский",           AgeMonths =  1, Price =   800, HealthStatus = "Здоров",                  ImagePath = @"Images\dzhungarskiy.jpg"     },
            new Animal { Species = "Кошка",   Breed = "Мейн-кун",              AgeMonths =  5, Price = 35000, HealthStatus = "Здорова, все прививки",   ImagePath = @"Images\mainkun.jpg"          },
            new Animal { Species = "Собака",  Breed = "Хаски",                 AgeMonths =  4, Price = 40000, HealthStatus = "Здоров, вакцинирован",    ImagePath = @"Images\i.webp"               }
        );
        await db.SaveChangesAsync();
    }
}
