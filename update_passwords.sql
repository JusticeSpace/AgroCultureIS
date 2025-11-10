-- ================================================================
-- SQL скрипт для обновления паролей пользователей
-- Заменяет открытые пароли на SHA256 хеши
-- ================================================================

USE [AgroCulture]
GO

-- Обновление пароля для пользователя 'admin' (пароль: admin)
-- SHA256("admin") = 8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918
UPDATE [dbo].[Users]
SET [PasswordHash] = '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918'
WHERE [Username] = 'admin'
GO

-- Обновление пароля для пользователя 'manager' (пароль: manager)
-- SHA256("manager") = 6ee4a469cd4e91053847f5d3fcb61dbcc91e8f0ef10be7748da4c4a1ba382d17
UPDATE [dbo].[Users]
SET [PasswordHash] = '6ee4a469cd4e91053847f5d3fcb61dbcc91e8f0ef10be7748da4c4a1ba382d17'
WHERE [Username] = 'manager'
GO

-- Обновление пароля для пользователя 'guest' (пароль: guest)
-- SHA256("guest") = 84983c60f7daadc1cb8698621f802c0d9f9a3c3c295c810748fb048115c186ec
UPDATE [dbo].[Users]
SET [PasswordHash] = '84983c60f7daadc1cb8698621f802c0d9f9a3c3c295c810748fb048115c186ec'
WHERE [Username] = 'guest'
GO

-- Проверка обновления
SELECT
    [UserId],
    [Username],
    [PasswordHash],
    [Role],
    [FullName],
    [IsActive]
FROM [dbo].[Users]
WHERE [Username] IN ('admin', 'manager', 'guest')
GO

PRINT 'Пароли успешно обновлены на SHA256 хеши'
PRINT 'Тестовые учетные записи:'
PRINT '  admin / admin (SHA256)'
PRINT '  manager / manager (SHA256)'
PRINT '  guest / guest (SHA256)'
GO
