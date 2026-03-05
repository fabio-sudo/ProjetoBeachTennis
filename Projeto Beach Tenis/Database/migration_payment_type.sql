USE ArenaManagementDB;
GO

-- Add PaymentType to Sales if not exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Sales') AND name = 'PaymentType')
BEGIN
    ALTER TABLE Sales ADD PaymentType NVARCHAR(50) DEFAULT 'Dinheiro';
END
GO
