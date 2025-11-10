-- ========================================
-- ПРОВЕРКА И ИСПРАВЛЕНИЕ ПАРОЛЯ МЕНЕДЖЕРА
-- ========================================

USE AgroCulture;
GO

PRINT '========================================';
PRINT 'ПРОВЕРКА ТЕКУЩИХ ПАРОЛЕЙ';
PRINT '========================================';

-- Показываем текущие пароли
SELECT
    UserId,
    Username,
    Role,
    IsActive,
    LEN(PasswordHash) AS HashLength,
    LEFT(PasswordHash, 20) + '...' AS HashPreview,
    CASE
        WHEN PasswordHash LIKE ' %' OR PasswordHash LIKE '% ' THEN 'ЕСТЬ ПРОБЕЛЫ!'
        ELSE 'OK'
    END AS HasSpaces
FROM Users
WHERE Username IN ('admin', 'manager', 'guest');
GO

PRINT '';
PRINT '========================================';
PRINT 'ИСПРАВЛЕНИЕ ПАРОЛЯ МЕНЕДЖЕРА';
PRINT '========================================';

-- Обновляем пароль менеджера на '1111' (с trim)
-- SHA256 хеш от '1111' = 0ffe1abd1a08215353c233d6e009613e95eec4253832a761af28ff37ac5a150c
UPDATE Users
SET PasswordHash = '0ffe1abd1a08215353c233d6e009613e95eec4253832a761af28ff37ac5a150c'
WHERE Username = 'manager';
GO

PRINT '✅ Пароль менеджера обновлён';
PRINT '';

PRINT '========================================';
PRINT 'ПРОВЕРКА ПОСЛЕ ОБНОВЛЕНИЯ';
PRINT '========================================';

SELECT
    Username,
    Role,
    IsActive,
    PasswordHash
FROM Users
WHERE Username = 'manager';
GO

PRINT '';
PRINT 'Теперь manager должен войти с паролем: 1111';
GO
