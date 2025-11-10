-- ========================================
-- ИСПРАВЛЕНИЕ ПРЕДСТАВЛЕНИЯ BookingsDetails
-- ========================================
-- Добавляем недостающие поля, которые ожидает Model.edmx

USE AgroCulture;
GO

PRINT '========================================';
PRINT 'УДАЛЕНИЕ СТАРОГО ПРЕДСТАВЛЕНИЯ';
PRINT '========================================';

-- Удаляем старое представление
IF OBJECT_ID('dbo.BookingsDetails', 'V') IS NOT NULL
    DROP VIEW [dbo].[BookingsDetails];
GO

PRINT '✅ Старое представление удалено';
PRINT '';
PRINT '========================================';
PRINT 'СОЗДАНИЕ НОВОГО ПРЕДСТАВЛЕНИЯ';
PRINT '========================================';
GO

-- Создаём новое представление со ВСЕМИ полями
CREATE VIEW [dbo].[BookingsDetails] AS
SELECT
    -- Поля из Bookings
    b.BookingId,
    b.CheckInDate,
    b.CheckOutDate,
    b.Nights,
    b.TotalPrice,
    b.Status,
    b.CreatedAt,

    -- Поля из Cabins
    c.CabinId,
    c.Name AS CabinName,
    c.PricePerNight,
    c.MaxGuests AS Capacity,  -- ⚠️ ВАЖНО: используем MaxGuests, но называем Capacity для совместимости с моделью

    -- Поля из Guests (отдельные + полное имя)
    g.GuestId,
    g.Surname AS GuestSurname,
    g.FirstName AS GuestFirstName,
    g.MiddleName AS GuestMiddleName,
    (g.Surname + ' ' + g.FirstName + ISNULL(' ' + g.MiddleName, '')) AS GuestFullName,
    g.Phone AS GuestPhone,
    g.Email AS GuestEmail,

    -- Поля из Users (кто создал бронирование - отдельные + полное имя)
    u.UserId AS CreatedByUserId,
    u.Surname AS CreatedBySurname,
    u.FirstName AS CreatedByFirstName,
    u.MiddleName AS CreatedByMiddleName,
    (u.Surname + ' ' + u.FirstName + ISNULL(' ' + u.MiddleName, '')) AS CreatedByFullName

FROM Bookings b
INNER JOIN Cabins c ON b.CabinId = c.CabinId
INNER JOIN Guests g ON b.GuestId = g.GuestId
LEFT JOIN Users u ON b.CreatedBy = u.UserId;
GO

PRINT '✅ Новое представление создано';
PRINT '';

PRINT '========================================';
PRINT 'ПРОВЕРКА ПРЕДСТАВЛЕНИЯ';
PRINT '========================================';

-- Проверяем, что представление работает
SELECT TOP 5
    BookingId,
    CabinName,
    Capacity,
    GuestSurname,
    GuestFirstName,
    GuestFullName,
    GuestPhone,
    CheckInDate,
    CheckOutDate,
    TotalPrice,
    Status
FROM [dbo].[BookingsDetails]
ORDER BY BookingId DESC;
GO

PRINT '';
PRINT '========================================';
PRINT '✅ ГОТОВО!';
PRINT '========================================';
PRINT 'Представление BookingsDetails обновлено';
PRINT 'Теперь оно возвращает все нужные поля!';
PRINT '========================================';
GO
