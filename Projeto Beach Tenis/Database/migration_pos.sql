USE ArenaManagementDB;
GO

-- 1. Create ProductCategories if not exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductCategories')
BEGIN
    CREATE TABLE ProductCategories (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL
    );
END
GO

-- 2. Add CategoryId to Products if not exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'CategoryId')
BEGIN
    ALTER TABLE Products ADD CategoryId INT NULL;
    ALTER TABLE Products ADD CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES ProductCategories(Id);
END
GO

-- 3. Create Customers if not exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Customers')
BEGIN
    CREATE TABLE Customers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Phone NVARCHAR(20) NULL,
        Email NVARCHAR(100) NULL,
        CreatedAt DATETIME DEFAULT GETDATE()
    );
END
GO

-- 4. Add CustomerId and StudentId to Sales if not exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Sales') AND name = 'CustomerId')
BEGIN
    ALTER TABLE Sales ADD CustomerId INT NULL;
    ALTER TABLE Sales ADD CONSTRAINT FK_Sales_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id);
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Sales') AND name = 'StudentId')
BEGIN
    ALTER TABLE Sales ADD StudentId INT NULL;
    ALTER TABLE Sales ADD CONSTRAINT FK_Sales_Students FOREIGN KEY (StudentId) REFERENCES Students(Id);
END
GO

-- 5. Seed default customer and categories if not present
IF NOT EXISTS (SELECT * FROM Customers WHERE Name = 'Consumidor')
BEGIN
    INSERT INTO Customers (Name) VALUES ('Consumidor');
END

IF NOT EXISTS (SELECT * FROM ProductCategories)
BEGIN
    INSERT INTO ProductCategories (Name) VALUES 
    ('Bebidas'), ('Snacks'), ('Energéticos'), ('Aluguel'), ('Outros');
END
GO
