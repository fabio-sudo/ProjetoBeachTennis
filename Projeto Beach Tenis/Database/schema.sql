-- Beach Tennis / Futvolei Arena Management System - Database Schema

CREATE DATABASE ArenaManagementDB;
GO

USE ArenaManagementDB;
GO

-- 1. Students Table
CREATE TABLE Students (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    PlanName NVARCHAR(50) NOT NULL, -- e.g., Basic, Pro, VIP
    StartDate DATE NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- 2. Courts Table
CREATE TABLE Courts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    SportType NVARCHAR(50) NOT NULL, -- e.g., Beach Tennis, Futvolei
    IsActive BIT DEFAULT 1
);
GO

-- 3. Reservations Table
CREATE TABLE Reservations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CourtId INT NOT NULL,
    CustomerName NVARCHAR(100) NOT NULL,
    ReservationDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    Price DECIMAL(10, 2) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CourtId) REFERENCES Courts(Id)
);
GO

-- Prevent overlapping reservations for the same court on the same date (Application layer will also enforce this)
-- Example Index for fast conflict checking
CREATE INDEX IX_Reservations_CourtDate ON Reservations (CourtId, ReservationDate);
GO

-- 4. ProductCategories Table
CREATE TABLE ProductCategories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL
);
GO

-- 5. Products Table (Bar)
CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Price DECIMAL(10, 2) NOT NULL,
    Stock INT NOT NULL DEFAULT 0,
    CategoryId INT NULL,
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (CategoryId) REFERENCES ProductCategories(Id)
);
GO

-- 6. Customers Table
CREATE TABLE Customers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NULL,
    Email NVARCHAR(100) NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- 7. Sales Table (Bar POS)
CREATE TABLE Sales (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SaleDate DATETIME NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(10, 2) NOT NULL,
    CustomerId INT NULL,
    StudentId INT NULL,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    FOREIGN KEY (StudentId) REFERENCES Students(Id)
);
GO

-- 8. SaleItems Table (Bar POS Details)
CREATE TABLE SaleItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SaleId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10, 2) NOT NULL,
    FOREIGN KEY (SaleId) REFERENCES Sales(Id),
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);
GO

-- 9. Seed Data
INSERT INTO ProductCategories (Name) VALUES 
('Bebidas'), ('Snacks'), ('Energéticos'), ('Aluguel'), ('Outros');

INSERT INTO Customers (Name) VALUES ('Consumidor');

INSERT INTO Courts (Name, SportType) VALUES 
('Court 1 - Premium Sand', 'Beach Tennis'),
('Court 2 - Standard', 'Futvolei'),
('Court 3 - Covered', 'Volei de Praia');

INSERT INTO Products (Name, Price, Stock, CategoryId) VALUES 
('Water 500ml', 5.00, 100, 1),
('Isotonic Drink', 10.00, 50, 3),
('Energy Bar', 8.00, 30, 2),
('Towel Rental', 15.00, 20, 4);

-- Insert Students
INSERT INTO Students (Name, Phone, Email, PlanName, StartDate) VALUES
('João Silva', '(11) 98888-7777', 'joao@email.com', 'VIP', GETDATE()),
('Maria Santos', '(11) 97777-6666', 'maria@email.com', 'Basic', GETDATE()),
('Pedro Oliveira', '(11) 96666-5555', 'pedro@email.com', 'Pro', GETDATE());

-- Insert Reservations for Today
DECLARE @Today DATE = CAST(GETDATE() AS DATE);
INSERT INTO Reservations (CourtId, CustomerName, ReservationDate, StartTime, EndTime, Price) VALUES
(1, 'João Silva', @Today, '08:00:00', '09:00:00', 80.00),
(1, 'Maria Santos', @Today, '18:00:00', '19:00:00', 100.00),
(2, 'Pedro Oliveira', @Today, '09:00:00', '10:00:00', 70.00);

-- Insert Sales for Today
INSERT INTO Sales (SaleDate, TotalAmount, CustomerId) VALUES (GETDATE(), 15.00, 1);
DECLARE @LastSaleId INT = SCOPE_IDENTITY();
INSERT INTO SaleItems (SaleId, ProductId, Quantity, UnitPrice) VALUES (@LastSaleId, 1, 1, 5.00);
INSERT INTO SaleItems (SaleId, ProductId, Quantity, UnitPrice) VALUES (@LastSaleId, 2, 1, 10.00);
GO
