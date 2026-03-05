-- ============================================================
-- Arena Beach Management System - Phase 2 Migration
-- Run this script against ArenaManagementDB
-- ============================================================

USE ArenaManagementDB;
GO

-- ============================================================
-- 1. Add CostPrice to Products (for purchase tracking)
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'CostPrice'
)
BEGIN
    ALTER TABLE Products ADD CostPrice DECIMAL(10,2) NOT NULL DEFAULT 0;
END
GO

-- ============================================================
-- 2. Suppliers
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Suppliers')
BEGIN
    CREATE TABLE Suppliers (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        Name        NVARCHAR(100) NOT NULL,
        Phone       NVARCHAR(20)  NULL,
        Email       NVARCHAR(100) NULL,
        CreatedAt   DATETIME      NOT NULL DEFAULT GETDATE()
    );
END
GO

-- ============================================================
-- 3. PurchaseOrders
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PurchaseOrders')
BEGIN
    CREATE TABLE PurchaseOrders (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        SupplierId  INT           NOT NULL,
        OrderDate   DATETIME      NOT NULL DEFAULT GETDATE(),
        Status      NVARCHAR(20)  NOT NULL DEFAULT 'Pending',  -- Pending | Received | Cancelled
        Notes       NVARCHAR(500) NULL,
        FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id)
    );
END
GO

-- ============================================================
-- 4. PurchaseOrderItems
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PurchaseOrderItems')
BEGIN
    CREATE TABLE PurchaseOrderItems (
        Id                INT IDENTITY(1,1) PRIMARY KEY,
        PurchaseOrderId   INT           NOT NULL,
        ProductId         INT           NOT NULL,
        Quantity          INT           NOT NULL,
        CostPrice         DECIMAL(10,2) NOT NULL,
        FOREIGN KEY (PurchaseOrderId) REFERENCES PurchaseOrders(Id),
        FOREIGN KEY (ProductId)       REFERENCES Products(Id)
    );
END
GO

-- ============================================================
-- 5. CashRegisters
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CashRegisters')
BEGIN
    CREATE TABLE CashRegisters (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        OpenedAt        DATETIME     NOT NULL DEFAULT GETDATE(),
        ClosedAt        DATETIME     NULL,
        OpeningAmount   DECIMAL(10,2) NOT NULL DEFAULT 0,
        ClosingAmount   DECIMAL(10,2) NULL,
        Status          NVARCHAR(10) NOT NULL DEFAULT 'Open'  -- Open | Closed
    );
END
GO

-- ============================================================
-- 6. CashTransactions
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CashTransactions')
BEGIN
    CREATE TABLE CashTransactions (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        CashRegisterId  INT           NOT NULL,
        Type            NVARCHAR(20)  NOT NULL,  -- Sale | Expense | Adjustment
        Amount          DECIMAL(10,2) NOT NULL,
        Description     NVARCHAR(300) NULL,
        SaleId          INT           NULL,       -- link to Sale if type = Sale
        CreatedAt       DATETIME      NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (CashRegisterId) REFERENCES CashRegisters(Id),
        FOREIGN KEY (SaleId)         REFERENCES Sales(Id)
    );
END
GO

-- ============================================================
-- 7. Tabs (Comandas)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Tabs')
BEGIN
    CREATE TABLE Tabs (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        CustomerId  INT          NULL,
        StudentId   INT          NULL,
        TableNumber NVARCHAR(20) NULL,
        OpenedAt    DATETIME     NOT NULL DEFAULT GETDATE(),
        ClosedAt    DATETIME     NULL,
        Status      NVARCHAR(10) NOT NULL DEFAULT 'Open',  -- Open | Closed
        SaleId      INT          NULL,   -- linked sale when closed
        FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
        FOREIGN KEY (StudentId)  REFERENCES Students(Id),
        FOREIGN KEY (SaleId)     REFERENCES Sales(Id)
    );
END
GO

-- ============================================================
-- 8. TabItems
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TabItems')
BEGIN
    CREATE TABLE TabItems (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        TabId       INT           NOT NULL,
        ProductId   INT           NOT NULL,
        Quantity    INT           NOT NULL,
        UnitPrice   DECIMAL(10,2) NOT NULL,
        AddedAt     DATETIME      NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (TabId)      REFERENCES Tabs(Id),
        FOREIGN KEY (ProductId)  REFERENCES Products(Id)
    );
END
GO

PRINT 'Phase 2 migration completed successfully.';
GO
