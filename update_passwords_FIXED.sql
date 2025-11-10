-- ================================================================
-- ИСПРАВЛЕННЫЙ SQL скрипт для обновления паролей
-- Учитывает реальные пароли из AAAAAAA.sql
-- ================================================================

USE [AgroCulture]
GO

PRINT '=========================================='
PRINT 'Начало обновления паролей...'
PRINT '=========================================='
GO

-- 1️⃣ Обновление пароля для admin (пароль: admin)
-- SHA256("admin") = 8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918
UPDATE [dbo].[Users]
SET [PasswordHash] = '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918'
WHERE [Username] = 'admin'
GO
PRINT '✅ Админ обновлен: admin / admin'
GO

-- 2️⃣ Обновление пароля для manager (пароль: 1111)
-- SHA256("1111") = 0ffe1abd1a08215353c233d6e009613e95eec4253832a761af28ff37ac5a150c
UPDATE [dbo].[Users]
SET [PasswordHash] = '0ffe1abd1a08215353c233d6e009613e95eec4253832a761af28ff37ac5a150c'
WHERE [Username] = 'manager'
GO
PRINT '✅ Менеджер обновлен: manager / 1111'
GO

-- 3️⃣ Обновление пароля для guest (пароль: guest) + АКТИВАЦИЯ
-- SHA256("guest") = 84983c60f7daadc1cb8698621f802c0d9f9a3c3c295c810748fb048115c186ec
UPDATE [dbo].[Users]
SET
    [PasswordHash] = '84983c60f7daadc1cb8698621f802c0d9f9a3c3c295c810748fb048115c186ec',
    [IsActive] = 1  -- АКТИВИРУЕМ гостя!
WHERE [Username] = 'guest'
GO
PRINT '✅ Гость обновлен и активирован: guest / guest'
GO

-- 4️⃣ Проверка результатов
PRINT ''
PRINT '=========================================='
PRINT 'Проверка обновленных пользователей:'
PRINT '=========================================='
GO

SELECT
    [UserId],
    [Username],
    [PasswordHash],
    [Role],
    [FullName],
    [IsActive]
FROM [dbo].[Users]
WHERE [Username] IN ('admin', 'manager', 'guest')
ORDER BY [UserId]
GO

PRINT ''
PRINT '=========================================='
PRINT '✅ Пароли успешно обновлены на SHA256 хеши!'
PRINT '=========================================='
PRINT ''
PRINT 'Тестовые учетные записи:'
PRINT '  admin / admin'
PRINT '  manager / 1111'
PRINT '  guest / guest'
PRINT ''
PRINT 'Все пользователи активны!'
PRINT '=========================================='
GO
