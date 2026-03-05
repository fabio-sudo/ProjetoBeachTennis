using ArenaBackend.Data;
using ArenaBackend.DTOs;
using ArenaBackend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArenaBackend.Services
{
    public class TabsService : ITabsService
    {
        private readonly ArenaDbContext _context;

        public TabsService(ArenaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> GetTabsAsync(string? status = null)
        {
            var query = _context.Tabs
                .Include(t => t.Customer)
                .Include(t => t.Student)
                .Include(t => t.Items).ThenInclude(i => i.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status == status);

            var tabs = await query.OrderByDescending(t => t.OpenedAt).ToListAsync();

            return tabs.Select(t => new
            {
                t.Id,
                t.TableNumber,
                t.Status,
                t.OpenedAt,
                t.ClosedAt,
                t.SaleId,
                CustomerName = t.Customer?.Name ?? t.Student?.Name ?? "Anônimo",
                Total = t.Items.Sum(i => i.UnitPrice * i.Quantity),
                ItemCount = t.Items.Count,
                Items = t.Items.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    ProductName = i.Product?.Name,
                    i.Quantity,
                    i.UnitPrice,
                    Subtotal = i.UnitPrice * i.Quantity
                })
            });
        }

        public async Task<object?> GetTabAsync(int id)
        {
            var tab = await _context.Tabs
                .Include(t => t.Customer)
                .Include(t => t.Student)
                .Include(t => t.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tab == null) return null;

            return new
            {
                tab.Id,
                tab.TableNumber,
                tab.Status,
                tab.OpenedAt,
                tab.ClosedAt,
                tab.SaleId,
                tab.CustomerId,
                tab.StudentId,
                CustomerName = tab.Customer?.Name ?? tab.Student?.Name ?? "Anônimo",
                Total = tab.Items.Sum(i => i.UnitPrice * i.Quantity),
                Items = tab.Items.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    ProductName = i.Product?.Name,
                    i.Quantity,
                    i.UnitPrice,
                    Subtotal = i.UnitPrice * i.Quantity
                })
            };
        }

        public async Task<object> OpenTabAsync(CreateTabDto dto)
        {
            var tab = new Tab
            {
                CustomerId = dto.CustomerId,
                StudentId = dto.StudentId,
                TableNumber = dto.TableNumber,
                OpenedAt = DateTime.Now,
                Status = "Open"
            };
            _context.Tabs.Add(tab);
            await _context.SaveChangesAsync();
            return new { tab.Id, tab.Status, tab.OpenedAt };
        }

        public async Task<string> AddItemAsync(int id, AddTabItemDto dto)
        {
            var tab = await _context.Tabs.Include(t => t.Items).FirstOrDefaultAsync(t => t.Id == id);
            if (tab == null) throw new InvalidOperationException("Comanda não encontrada.");
            if (tab.Status != "Open") throw new InvalidOperationException("Esta comanda já está fechada.");

            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null) throw new InvalidOperationException("Produto não encontrado.");
            if (!product.IsActive) throw new InvalidOperationException("Produto inativo.");
            if (product.Stock < dto.Quantity) throw new InvalidOperationException($"Estoque insuficiente. Disponível: {product.Stock}");

            // Deduct stock immediately
            product.Stock -= dto.Quantity;

            // Check if same product already in tab — merge
            var existing = tab.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (existing != null)
                existing.Quantity += dto.Quantity;
            else
                tab.Items.Add(new TabItem { TabId = id, ProductId = dto.ProductId, Quantity = dto.Quantity, UnitPrice = product.Price });

            await _context.SaveChangesAsync();
            return "Item adicionado à comanda.";
        }

        public async Task<string> RemoveItemAsync(int id, int itemId)
        {
            var tab = await _context.Tabs.Include(t => t.Items).FirstOrDefaultAsync(t => t.Id == id);
            if (tab == null) throw new InvalidOperationException("Comanda não encontrada.");
            if (tab.Status != "Open") throw new InvalidOperationException("Esta comanda já está fechada.");

            var item = tab.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) throw new InvalidOperationException("Item não encontrado.");

            // Restore stock
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product != null) product.Stock += item.Quantity;

            _context.TabItems.Remove(item);
            await _context.SaveChangesAsync();
            return "Item removido da comanda.";
        }

        public async Task<object> CloseTabAsync(int id, CloseTabDto dto)
        {
            var tab = await _context.Tabs
                .Include(t => t.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tab == null) throw new InvalidOperationException("Comanda não encontrada.");
            if (tab.Status != "Open") throw new InvalidOperationException("Esta comanda já está fechada.");
            if (!tab.Items.Any()) throw new InvalidOperationException("Comanda está vazia.");

            // Create a sale from the tab
            var sale = new Sale
            {
                SaleDate = DateTime.Now,
                PaymentType = dto.PaymentType,
                CustomerId = tab.CustomerId,
                StudentId = tab.StudentId,
                Items = tab.Items.Select(i => new SaleItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };
            sale.TotalAmount = sale.Items.Sum(i => i.UnitPrice * i.Quantity);
            _context.Sales.Add(sale);

            // Close tab
            tab.Status = "Closed";
            tab.ClosedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // Link sale to tab
            tab.SaleId = sale.Id;

            // Register in open cash register if any
            var openRegister = await _context.CashRegisters.FirstOrDefaultAsync(cr => cr.Status == "Open");
            if (openRegister != null)
            {
                _context.CashTransactions.Add(new CashTransaction
                {
                    CashRegisterId = openRegister.Id,
                    Type = "Sale",
                    Amount = sale.TotalAmount,
                    Description = $"Fechamento de Comanda #{tab.Id}",
                    SaleId = sale.Id,
                    CreatedAt = DateTime.Now
                });
            }
            await _context.SaveChangesAsync();

            return new { saleId = sale.Id, total = sale.TotalAmount };
        }
    }
}
