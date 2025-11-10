-- ========================================
-- Исправление дублирующих внешних ключей в таблице CabinAmenities
-- ========================================

USE AgroCulture;
GO

-- Проверяем существующие FK
SELECT
    fk.name AS FK_Name,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable
FROM sys.foreign_keys AS fk
INNER JOIN sys.foreign_key_columns AS fkc
    ON fk.object_id = fkc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) = 'CabinAmenities'
ORDER BY fk.name;
GO

PRINT '========================================';
PRINT 'Удаление старых автогенерированных FK...';
PRINT '========================================';

-- Удаляем старые FK (с автогенерированными именами)
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK__CabinAmen__Ameni__5165187F')
BEGIN
    ALTER TABLE CabinAmenities DROP CONSTRAINT FK__CabinAmen__Ameni__5165187F;
    PRINT '✓ Удалён FK__CabinAmen__Ameni__5165187F';
END
ELSE
    PRINT '- FK__CabinAmen__Ameni__5165187F не найден';

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK__CabinAmen__Cabin__52593CB8')
BEGIN
    ALTER TABLE CabinAmenities DROP CONSTRAINT FK__CabinAmen__Cabin__52593CB8;
    PRINT '✓ Удалён FK__CabinAmen__Cabin__52593CB8';
END
ELSE
    PRINT '- FK__CabinAmen__Cabin__52593CB8 не найден';

PRINT '';
PRINT '========================================';
PRINT 'Проверка оставшихся FK...';
PRINT '========================================';

-- Проверяем что остались только правильные FK
SELECT
    fk.name AS FK_Name,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable
FROM sys.foreign_keys AS fk
INNER JOIN sys.foreign_key_columns AS fkc
    ON fk.object_id = fkc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) = 'CabinAmenities'
ORDER BY fk.name;
GO

PRINT '';
PRINT '========================================';
PRINT '✓ Готово! Теперь обновите модель в Visual Studio:';
PRINT '  1. Откройте Model.edmx';
PRINT '  2. ПКМ → Update Model from Database';
PRINT '  3. Build → Rebuild Solution';
PRINT '========================================';
