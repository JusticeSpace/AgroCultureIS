-- ========================================
-- СУПЕР-ДИАГНОСТИКА ПАРОЛЯ МЕНЕДЖЕРА
-- ========================================

USE AgroCulture;
GO

PRINT '========================================';
PRINT '1. ТЕКУЩИЙ ХЕШ В БД';
PRINT '========================================';

DECLARE @CurrentHash NVARCHAR(256);
DECLARE @ExpectedHash NVARCHAR(256) = '0ffe1abd1a08215353c233d6e009613e95eec4253832a761af28ff37ac5a150c';

SELECT @CurrentHash = PasswordHash FROM Users WHERE Username = 'manager';

SELECT
    Username,
    LEN(PasswordHash) AS [Длина хеша],
    PasswordHash AS [Текущий хеш в БД],
    @ExpectedHash AS [Правильный хеш (1111)],
    CASE
        WHEN PasswordHash = @ExpectedHash THEN '✅ СОВПАДАЕТ'
        WHEN LTRIM(RTRIM(PasswordHash)) = @ExpectedHash THEN '⚠️ ЕСТЬ ПРОБЕЛЫ'
        WHEN LOWER(PasswordHash) = @ExpectedHash THEN '⚠️ НЕПРАВИЛЬНЫЙ РЕГИСТР'
        ELSE '❌ ХЕШ ПОЛНОСТЬЮ ДРУГОЙ'
    END AS [Статус],
    CASE
        WHEN PasswordHash LIKE ' %' THEN 'Пробел в начале'
        WHEN PasswordHash LIKE '% ' THEN 'Пробел в конце'
        ELSE 'Нет пробелов'
    END AS [Проверка пробелов]
FROM Users
WHERE Username = 'manager';
GO

PRINT '';
PRINT '========================================';
PRINT '2. СРАВНЕНИЕ БАЙТ ЗА БАЙТОМ';
PRINT '========================================';

DECLARE @CurrentHash2 NVARCHAR(256);
DECLARE @ExpectedHash2 NVARCHAR(256) = '0ffe1abd1a08215353c233d6e009613e95eec4253832a761af28ff37ac5a150c';

SELECT @CurrentHash2 = PasswordHash FROM Users WHERE Username = 'manager';

SELECT
    'Первые 10 символов БД' AS Описание,
    LEFT(@CurrentHash2, 10) AS Значение,
    LEN(LEFT(@CurrentHash2, 10)) AS Длина
UNION ALL
SELECT
    'Первые 10 символов ПРАВИЛЬНЫЙ',
    LEFT(@ExpectedHash2, 10),
    LEN(LEFT(@ExpectedHash2, 10))
UNION ALL
SELECT
    'Последние 10 символов БД',
    RIGHT(@CurrentHash2, 10),
    LEN(RIGHT(@CurrentHash2, 10))
UNION ALL
SELECT
    'Последние 10 символов ПРАВИЛЬНЫЙ',
    RIGHT(@ExpectedHash2, 10),
    LEN(RIGHT(@ExpectedHash2, 10));
GO

PRINT '';
PRINT '========================================';
PRINT '3. АВТОМАТИЧЕСКОЕ ИСПРАВЛЕНИЕ';
PRINT '========================================';

-- Исправляем хеш
UPDATE Users
SET PasswordHash = '0ffe1abd1a08215353c233d6e009613e95eec4253832a761af28ff37ac5a150c'
WHERE Username = 'manager';
GO

PRINT '✅ Хеш обновлён на правильный!';
PRINT '';

PRINT '========================================';
PRINT '4. ФИНАЛЬНАЯ ПРОВЕРКА';
PRINT '========================================';

SELECT
    Username,
    Role,
    IsActive,
    PasswordHash,
    CASE
        WHEN PasswordHash = '0ffe1abd1a08215353c233d6e009613e95eec4253832a761af28ff37ac5a150c'
        THEN '✅✅✅ ПРАВИЛЬНЫЙ ХЕШ!'
        ELSE '❌ ВСЁ ЕЩЁ НЕПРАВИЛЬНЫЙ'
    END AS [Финальный статус]
FROM Users
WHERE Username = 'manager';
GO

PRINT '';
PRINT '========================================';
PRINT 'ГОТОВО!';
PRINT '========================================';
PRINT 'Теперь войди с паролем: 1111';
PRINT '========================================';
GO
