-- ========================================
-- ПОЛНОЕ ПЕРЕСОЗДАНИЕ ПОЛЬЗОВАТЕЛЯ MANAGER
-- ========================================

USE AgroCulture;
GO

PRINT '========================================';
PRINT 'ШАГ 1: УДАЛЕНИЕ СТАРОГО MANAGER';
PRINT '========================================';

-- Удаляем старого менеджера
DELETE FROM Users WHERE Username = 'manager';
GO

PRINT '✅ Старый manager удалён';
PRINT '';

PRINT '========================================';
PRINT 'ШАГ 2: СОЗДАНИЕ НОВОГО MANAGER';
PRINT '========================================';

-- Создаём нового менеджера с нуля
INSERT INTO Users (Username, PasswordHash, Role, Phone, Email, IsActive, Surname, FirstName, MiddleName, CreatedAt)
VALUES (
    'manager',                                                                      -- Username
    '0ffe1abd1a08215353c233d6e009613e95eec4253832a761af28ff37ac5a150c',        -- PasswordHash для '1111'
    'manager',                                                                      -- Role
    '+7 (900) 123-45-67',                                                          -- Phone
    'manager@agroculture.ru',                                                      -- Email
    1,                                                                              -- IsActive
    'Менеджеров',                                                                  -- Surname
    'Менеджер',                                                                    -- FirstName
    'Менеджерович',                                                                -- MiddleName
    GETDATE()                                                                       -- CreatedAt
);
GO

PRINT '✅ Новый manager создан';
PRINT '';

PRINT '========================================';
PRINT 'ШАГ 3: ПРОВЕРКА';
PRINT '========================================';

SELECT
    UserId,
    Username,
    Role,
    IsActive,
    LEN(PasswordHash) AS HashLength,
    PasswordHash,
    Surname + ' ' + FirstName AS FullName
FROM Users
WHERE Username = 'manager';
GO

PRINT '';
PRINT '✅ ГОТОВО! Теперь входи:';
PRINT 'Логин: manager';
PRINT 'Пароль: 1111';
GO
