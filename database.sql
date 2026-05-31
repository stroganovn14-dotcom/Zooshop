-- ============================================================
-- АИС Зоомагазин — скрипт создания и заполнения БД
-- SQL Server 2019+ / Azure SQL
-- Запускать от имени пользователя с правами CREATE DATABASE
-- ============================================================

USE master;
GO

-- Создание базы данных (если не существует)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'ZooShop')
BEGIN
    CREATE DATABASE ZooShop
        COLLATE Cyrillic_General_CI_AS;
    PRINT 'База данных ZooShop создана.';
END
GO

USE ZooShop;
GO

-- ============================================================
-- ТАБЛИЦЫ
-- ============================================================

-- Пользователи (Role: 1=Seller, 2=Manager, 3=Admin)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
BEGIN
    CREATE TABLE Users (
        Id           INT           IDENTITY(1,1) PRIMARY KEY,
        FullName     NVARCHAR(200) NOT NULL,
        Login        NVARCHAR(100) NOT NULL,
        PasswordHash NVARCHAR(256) NOT NULL,
        Role         INT           NOT NULL DEFAULT 1,  -- UserRole enum
        CONSTRAINT UQ_Users_Login UNIQUE (Login),
        CONSTRAINT CK_Users_Role CHECK (Role IN (1, 2, 3))
    );
    PRINT 'Таблица Users создана.';
END
GO

-- Товары
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products')
BEGIN
    CREATE TABLE Products (
        Id             INT             IDENTITY(1,1) PRIMARY KEY,
        Name           NVARCHAR(200)   NOT NULL,
        Category       NVARCHAR(100)   NOT NULL,
        Price          DECIMAL(18, 2)  NOT NULL DEFAULT 0,
        Stock          INT             NOT NULL DEFAULT 0,
        ExpirationDate DATETIME2       NULL,
        ImagePath      NVARCHAR(500)   NULL,
        CONSTRAINT CK_Products_Price CHECK (Price >= 0),
        CONSTRAINT CK_Products_Stock CHECK (Stock >= 0)
    );
    PRINT 'Таблица Products создана.';
END
GO

-- Животные (Age в месяцах)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Animals')
BEGIN
    CREATE TABLE Animals (
        Id           INT            IDENTITY(1,1) PRIMARY KEY,
        Species      NVARCHAR(100)  NOT NULL,
        Breed        NVARCHAR(100)  NULL,
        AgeMonths    INT            NOT NULL DEFAULT 0,
        Price        DECIMAL(18, 2) NOT NULL DEFAULT 0,
        HealthStatus NVARCHAR(300)  NOT NULL DEFAULT N'',
        ImagePath    NVARCHAR(500)  NULL,
        CONSTRAINT CK_Animals_Price CHECK (Price >= 0),
        CONSTRAINT CK_Animals_Age   CHECK (AgeMonths >= 0)
    );
    PRINT 'Таблица Animals создана.';
END
GO

-- Продажи (ItemType: 0=Product, 1=Animal)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Sales')
BEGIN
    CREATE TABLE Sales (
        Id         INT            IDENTITY(1,1) PRIMARY KEY,
        ItemId     INT            NOT NULL,
        ItemType   INT            NOT NULL DEFAULT 0,
        TotalSum   DECIMAL(18, 2) NOT NULL DEFAULT 0,
        SaleDate   DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
        SellerId   INT            NOT NULL,
        OwnerName  NVARCHAR(200)  NULL,
        OwnerPhone NVARCHAR(20)   NULL,
        CONSTRAINT FK_Sales_Seller FOREIGN KEY (SellerId)
            REFERENCES Users(Id) ON DELETE NO ACTION,
        CONSTRAINT CK_Sales_ItemType CHECK (ItemType IN (0, 1)),
        CONSTRAINT CK_Sales_TotalSum CHECK (TotalSum >= 0)
    );

    -- Индекс для быстрой фильтрации по дате
    CREATE NONCLUSTERED INDEX IX_Sales_SaleDate
        ON Sales (SaleDate DESC);

    -- Индекс для фильтрации по продавцу
    CREATE NONCLUSTERED INDEX IX_Sales_SellerId
        ON Sales (SellerId);

    PRINT 'Таблица Sales создана.';
END
GO

-- ============================================================
-- НАЧАЛЬНЫЕ ДАННЫЕ
-- ============================================================

-- Пользователи. Хэши сгенерированы BCrypt.Net cost=11.
IF NOT EXISTS (SELECT 1 FROM Users WHERE Login = 'admin')
BEGIN
    INSERT INTO Users (FullName, Login, PasswordHash, Role)
    VALUES (
        N'Главный Администратор',
        'admin',
        'admin',  -- Admin123!
        3
    );
    PRINT 'Создан пользователь admin (Admin123!).';
END

IF NOT EXISTS (SELECT 1 FROM Users WHERE Login = 'manager1')
BEGIN
    INSERT INTO Users (FullName, Login, PasswordHash, Role)
    VALUES (
        N'Иванов Иван Иванович',
        'manager',
        'manager',
        2
    );
    PRINT 'Создан пользователь manager1.';
END

IF NOT EXISTS (SELECT 1 FROM Users WHERE Login = 'seller1')
BEGIN
    INSERT INTO Users (FullName, Login, PasswordHash, Role)
    VALUES (
        N'Петрова Мария Сергеевна',
        'seller',
        'seller',
        1
    );
    PRINT 'Создан пользователь seller1.';
END
GO

-- Товары
IF NOT EXISTS (SELECT 1 FROM Products)
BEGIN
    INSERT INTO Products (Name, Category, Price, Stock, ExpirationDate) VALUES
    -- Товар с истекающим сроком (через 3 дня — будет красный)
    (N'Корм для кошек Royal Canin 2кг',  N'Корма',          1299.00, 50,
        DATEADD(day, 3,   GETDATE())),
    -- Товар нормальный
    (N'Корм для собак Purina Pro Plan',   N'Корма',           899.00, 30,
        DATEADD(day, 180, GETDATE())),
    -- Скоро истечёт (5 дней — янтарный)
    (N'Витамины для грызунов Beaphar',    N'Витамины',        450.00, 20,
        DATEADD(day, 5,   GETDATE())),
    -- Уже просрочен (красный)
    (N'Лакомства для кошек Dreamies',     N'Лакомства',       199.00, 15,
        DATEADD(day, -2,  GETDATE())),
    -- Без срока годности
    (N'Игрушка-мышь для кошки',           N'Игрушки',         299.00, 100, NULL),
    (N'Аквариум 50л с крышкой',           N'Аквариумистика', 3500.00,   5, NULL),
    (N'Лежанка ортопедическая 70x50',     N'Аксессуары',     1800.00,  15, NULL),
    (N'Шлейка для кошек размер S',        N'Аксессуары',      350.00,  40, NULL),
    (N'Наполнитель силикагелевый 5л',     N'Гигиена',         580.00,  60,
        DATEADD(day, 365, GETDATE())),
    (N'Антипаразитарный ошейник Beaphar', N'Ветеринария',     490.00,  25,
        DATEADD(day, 90,  GETDATE()));

    PRINT 'Товары добавлены.';
END
GO

-- Животные
IF NOT EXISTS (SELECT 1 FROM Animals)
BEGIN
    INSERT INTO Animals (Species, Breed, AgeMonths, Price, HealthStatus) VALUES
    (N'Кошка',    N'Шотландская вислоухая',  3, 25000.00, N'Здорова, вакцинирована, чипирована'),
    (N'Собака',   N'Лабрадор-ретривер',      6, 45000.00, N'Здоров, вакцинирован, с документами'),
    (N'Попугай',  N'Волнистый',             12,  3500.00, N'Здоров, ручной'),
    (N'Кролик',   N'Карликовый баран',        2,  5000.00, N'Здоров, вакцинирован'),
    (N'Хомяк',    N'Джунгарский',             1,   800.00, N'Здоров'),
    (N'Черепаха', N'Среднеазиатская',        24,  4500.00, N'Здорова, активная'),
    (N'Кошка',    N'Мейн-кун',               5, 35000.00, N'Здорова, все прививки'),
    (N'Собака',   N'Хаски',                  4, 40000.00, N'Здоров, вакцинирован');

    PRINT 'Животные добавлены.';
END
GO

PRINT '=== Инициализация базы данных завершена ===';
PRINT 'Логины и пароли:';
PRINT '  admin    / Admin123!';
PRINT '  manager1 / Manager123!';
PRINT '  seller1  / Seller123!';
GO
