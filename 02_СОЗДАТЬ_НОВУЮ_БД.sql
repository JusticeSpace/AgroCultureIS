-- ================================================================
-- ШАГ 2: СОЗДАНИЕ НОВОЙ БАЗЫ ДАННЫХ (упрощенный вариант)
-- Выполните ПОСЛЕ удаления старой БД
-- ================================================================

USE [master]
GO

-- Создаем базу данных (SQL Server сам выберет путь)
CREATE DATABASE [AgroCulture]
GO

USE [AgroCulture]
GO

PRINT '✅ База данных создана, начинаем создание таблиц...'
GO

-- ================================================================
-- СОЗДАНИЕ ТАБЛИЦ
-- ================================================================

-- Таблица: Users (Пользователи)
CREATE TABLE [dbo].[Users](
    [UserId] [int] IDENTITY(1,1) NOT NULL,
    [Username] [nvarchar](50) NOT NULL,
    [PasswordHash] [nvarchar](64) NOT NULL,
    [Surname] [nvarchar](50) NOT NULL,
    [FirstName] [nvarchar](50) NOT NULL,
    [MiddleName] [nvarchar](50) NULL,
    [Role] [nvarchar](20) NOT NULL,
    [Phone] [nvarchar](20) NULL,
    [Email] [nvarchar](100) NULL,
    [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
    [IsActive] [bit] NOT NULL DEFAULT 1,
    PRIMARY KEY ([UserId])
)
GO

PRINT '✅ Таблица Users создана'
GO

-- Таблица: Guests (Гости)
CREATE TABLE [dbo].[Guests](
    [GuestId] [int] IDENTITY(1,1) NOT NULL,
    [Surname] [nvarchar](50) NOT NULL,
    [FirstName] [nvarchar](50) NOT NULL,
    [MiddleName] [nvarchar](50) NULL,
    [Phone] [nvarchar](20) NOT NULL,
    [Email] [nvarchar](100) NULL,
    [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
    PRIMARY KEY ([GuestId])
)
GO

PRINT '✅ Таблица Guests создана'
GO

-- Таблица: Cabins (Домики)
CREATE TABLE [dbo].[Cabins](
    [CabinId] [int] IDENTITY(1,1) NOT NULL,
    [Name] [nvarchar](100) NOT NULL,
    [Description] [nvarchar](500) NULL,
    [MaxGuests] [int] NOT NULL,
    [PricePerNight] [decimal](10, 2) NOT NULL,
    [ImageUrl] [nvarchar](255) NULL,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    PRIMARY KEY ([CabinId])
)
GO

PRINT '✅ Таблица Cabins создана'
GO

-- Таблица: Amenities (Удобства)
CREATE TABLE [dbo].[Amenities](
    [AmenityId] [int] IDENTITY(1,1) NOT NULL,
    [Name] [nvarchar](50) NOT NULL,
    [Icon] [nvarchar](50) NULL,
    PRIMARY KEY ([AmenityId])
)
GO

PRINT '✅ Таблица Amenities создана'
GO

-- Таблица: Bookings (Бронирования)
CREATE TABLE [dbo].[Bookings](
    [BookingId] [int] IDENTITY(1,1) NOT NULL,
    [CabinId] [int] NOT NULL,
    [GuestId] [int] NOT NULL,
    [CheckInDate] [date] NOT NULL,
    [CheckOutDate] [date] NOT NULL,
    [Nights] [int] NOT NULL,
    [TotalPrice] [decimal](10, 2) NOT NULL,
    [Status] [nvarchar](20) NOT NULL DEFAULT 'active',
    [CreatedBy] [int] NULL,
    [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
    PRIMARY KEY ([BookingId]),
    FOREIGN KEY ([CabinId]) REFERENCES [Cabins]([CabinId]),
    FOREIGN KEY ([GuestId]) REFERENCES [Guests]([GuestId]),
    FOREIGN KEY ([CreatedBy]) REFERENCES [Users]([UserId])
)
GO

PRINT '✅ Таблица Bookings создана'
GO

-- Таблица: CabinAmenities (Связь домиков и удобств)
CREATE TABLE [dbo].[CabinAmenities](
    [AmenityId] [int] NOT NULL,
    [CabinId] [int] NOT NULL,
    PRIMARY KEY ([AmenityId], [CabinId]),
    FOREIGN KEY ([AmenityId]) REFERENCES [Amenities]([AmenityId]) ON DELETE CASCADE,
    FOREIGN KEY ([CabinId]) REFERENCES [Cabins]([CabinId]) ON DELETE CASCADE
)
GO

PRINT '✅ Таблица CabinAmenities создана'
GO

-- ================================================================
-- ВСТАВКА ТЕСТОВЫХ ДАННЫХ
-- ================================================================

PRINT ''
PRINT 'Добавляем тестовые данные...'
GO

-- Пользователи (с ОТКРЫТЫМИ паролями - обновим позже)
SET IDENTITY_INSERT [Users] ON
INSERT INTO [Users] ([UserId], [Username], [PasswordHash], [Surname], [FirstName], [MiddleName], [Role], [Phone], [Email], [CreatedAt], [IsActive])
VALUES
    (1, N'admin', N'admin', N'Петров', N'Петр', N'Петрович', N'admin', N'+7 (999) 001-00-01', N'admin@lesusadba.ru', GETDATE(), 1),
    (2, N'manager', N'1111', N'Иванова', N'Мария', N'Сергеевна', N'manager', N'+7 (999) 002-00-02', N'ivanova@lesusadba.ru', GETDATE(), 1),
    (3, N'guest', N'guest', N'Смирнов', N'Алексей', N'Петрович', N'guest', N'+7 (999) 003-00-03', N'smirnov@mail.ru', GETDATE(), 1)
SET IDENTITY_INSERT [Users] OFF
GO

PRINT '✅ Пользователи добавлены'
GO

-- Гости
SET IDENTITY_INSERT [Guests] ON
INSERT INTO [Guests] ([GuestId], [Surname], [FirstName], [MiddleName], [Phone], [Email], [CreatedAt])
VALUES
    (1, N'Козлов', N'Иван', N'Петрович', N'+7 (999) 111-11-11', N'kozlov@mail.ru', GETDATE()),
    (2, N'Морозова', N'Анна', N'Ивановна', N'+7 (999) 222-22-22', N'morozova@mail.ru', GETDATE()),
    (3, N'Соколов', N'Дмитрий', N'Александрович', N'+7 (999) 333-33-33', N'sokolov@mail.ru', GETDATE())
SET IDENTITY_INSERT [Guests] OFF
GO

PRINT '✅ Гости добавлены'
GO

-- Домики
SET IDENTITY_INSERT [Cabins] ON
INSERT INTO [Cabins] ([CabinId], [Name], [Description], [MaxGuests], [PricePerNight], [ImageUrl], [IsActive])
VALUES
    (1, N'Лесной уют', N'Уютный домик в сосновом бору', 4, 3500.00, NULL, 1),
    (2, N'Озерная дача', N'Домик с видом на озеро', 6, 5000.00, NULL, 1),
    (3, N'Горный приют', N'Комфортный домик в горах', 8, 7000.00, NULL, 1),
    (4, N'Семейное гнёздышко', N'Идеально для семейного отдыха', 5, 4500.00, NULL, 1)
SET IDENTITY_INSERT [Cabins] OFF
GO

PRINT '✅ Домики добавлены'
GO

-- Удобства
SET IDENTITY_INSERT [Amenities] ON
INSERT INTO [Amenities] ([AmenityId], [Name], [Icon])
VALUES
    (1, N'Wi-Fi', N'Wifi'),
    (2, N'Кухня', N'Silverware'),
    (3, N'Камин', N'Fire'),
    (4, N'Баня', N'Shower'),
    (5, N'Парковка', N'Car'),
    (6, N'Терраса', N'Deck'),
    (7, N'Мангал', N'Grill'),
    (8, N'ТВ', N'Television')
SET IDENTITY_INSERT [Amenities] OFF
GO

PRINT '✅ Удобства добавлены'
GO

-- Связи домиков и удобств
INSERT INTO [CabinAmenities] ([AmenityId], [CabinId])
VALUES
    (1, 1), (2, 1), (3, 1), (4, 1),  -- Лесной уют
    (1, 2), (2, 2), (5, 2), (6, 2),  -- Озерная дача
    (1, 3), (2, 3), (3, 3), (7, 3),  -- Горный приют
    (1, 4), (2, 4), (4, 4), (8, 4)   -- Семейное гнёздышко
GO

PRINT '✅ Удобства привязаны к домикам'
GO

-- Тестовое бронирование
SET IDENTITY_INSERT [Bookings] ON
INSERT INTO [Bookings] ([BookingId], [CabinId], [GuestId], [CheckInDate], [CheckOutDate], [Nights], [TotalPrice], [Status], [CreatedBy], [CreatedAt])
VALUES
    (1, 1, 1, '2025-11-15', '2025-11-18', 3, 10500.00, 'active', 1, GETDATE())
SET IDENTITY_INSERT [Bookings] OFF
GO

PRINT '✅ Тестовое бронирование добавлено'
GO

-- ================================================================
-- СОЗДАНИЕ ПРЕДСТАВЛЕНИЙ (Views)
-- ================================================================

-- View: BookingsDetails
CREATE VIEW [dbo].[BookingsDetails] AS
SELECT
    b.BookingId,
    b.CheckInDate,
    b.CheckOutDate,
    b.Nights,
    b.TotalPrice,
    b.Status,
    b.CreatedAt,
    c.CabinId,
    c.Name AS CabinName,
    c.PricePerNight,
    g.GuestId,
    (g.Surname + ' ' + g.FirstName + ISNULL(' ' + g.MiddleName, '')) AS GuestFullName,
    g.Phone AS GuestPhone,
    g.Email AS GuestEmail,
    u.UserId AS CreatedBy,
    (u.Surname + ' ' + u.FirstName + ISNULL(' ' + u.MiddleName, '')) AS CreatedByName
FROM Bookings b
INNER JOIN Cabins c ON b.CabinId = c.CabinId
INNER JOIN Guests g ON b.GuestId = g.GuestId
LEFT JOIN Users u ON b.CreatedBy = u.UserId
GO

PRINT '✅ View BookingsDetails создан'
GO

-- View: CabinsWithAmenities
CREATE VIEW [dbo].[CabinsWithAmenities] AS
SELECT
    c.CabinId,
    c.Name,
    c.Description,
    c.MaxGuests,
    c.PricePerNight,
    c.ImageUrl,
    c.IsActive,
    STUFF((
        SELECT ', ' + a.Name
        FROM CabinAmenities ca
        INNER JOIN Amenities a ON ca.AmenityId = a.AmenityId
        WHERE ca.CabinId = c.CabinId
        FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS Amenities
FROM Cabins c
GO

PRINT '✅ View CabinsWithAmenities создан'
GO

-- ================================================================
-- СОЗДАНИЕ ИНДЕКСОВ
-- ================================================================

CREATE INDEX IX_Users_Username ON Users(Username)
CREATE INDEX IX_Users_Role ON Users(Role)
CREATE INDEX IX_Bookings_Status ON Bookings(Status)
CREATE INDEX IX_Bookings_Dates ON Bookings(CheckInDate, CheckOutDate)
CREATE INDEX IX_Cabins_Name ON Cabins(Name)
GO

PRINT '✅ Индексы созданы'
GO

-- ================================================================
-- ИТОГО
-- ================================================================

PRINT ''
PRINT '=========================================='
PRINT '✅✅✅ БАЗА ДАННЫХ СОЗДАНА УСПЕШНО! ✅✅✅'
PRINT '=========================================='
PRINT ''
PRINT 'Создано:'
PRINT '  • 6 таблиц'
PRINT '  • 2 представления (Views)'
PRINT '  • Индексы для быстрого поиска'
PRINT ''
PRINT 'Тестовые данные:'
PRINT '  • 3 пользователя (admin, manager, guest)'
PRINT '  • 3 гостя'
PRINT '  • 4 домика'
PRINT '  • 8 удобств'
PRINT '  • 1 бронирование'
PRINT ''
PRINT '⚠️ ВАЖНО: Пароли в ОТКРЫТОМ виде!'
PRINT '   Теперь выполните: update_passwords_FIXED.sql'
PRINT '=========================================='
GO
