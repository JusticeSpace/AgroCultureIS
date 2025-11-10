-- ================================================================
-- ШАГ 1: УДАЛЕНИЕ СТАРОЙ БАЗЫ ДАННЫХ
-- Выполните этот скрипт ПЕРВЫМ!
-- ================================================================

USE [master]
GO

-- Закрываем все активные подключения к базе
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'AgroCulture')
BEGIN
    ALTER DATABASE [AgroCulture] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [AgroCulture];
    PRINT '✅ База данных AgroCulture удалена'
END
ELSE
BEGIN
    PRINT 'ℹ️ База данных AgroCulture не существует'
END
GO

PRINT ''
PRINT '=========================================='
PRINT '✅ Готово! Теперь выполните скрипт'
PRINT '   02_СОЗДАТЬ_НОВУЮ_БД.sql'
PRINT '=========================================='
GO
