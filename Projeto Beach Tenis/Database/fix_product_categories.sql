USE ArenaManagementDB;
GO

-- Link existing products to categories
UPDATE Products SET CategoryId = (SELECT Id FROM ProductCategories WHERE Name = 'Bebidas') WHERE Name LIKE '%Water%' OR Name LIKE '%Drink%';
UPDATE Products SET CategoryId = (SELECT Id FROM ProductCategories WHERE Name = 'Snacks') WHERE Name LIKE '%Bar%' OR Name LIKE '%Snack%';
UPDATE Products SET CategoryId = (SELECT Id FROM ProductCategories WHERE Name = 'Aluguel') WHERE Name LIKE '%Rental%';
UPDATE Products SET CategoryId = (SELECT Id FROM ProductCategories WHERE Name = 'Energéticos') WHERE Name LIKE '%Energy%';
GO
