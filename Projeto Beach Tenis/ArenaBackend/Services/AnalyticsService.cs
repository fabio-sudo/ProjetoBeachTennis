using ArenaBackend.Data;
using ArenaBackend.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArenaBackend.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ArenaDbContext _context;

        public AnalyticsService(ArenaDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetDashboardAsync()
        {
            var now = DateTime.Now;
            var today = now.Date;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);

            var salesToday = await _context.Sales
                .Where(s => s.SaleDate >= today)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

            var salesMonth = await _context.Sales
                .Where(s => s.SaleDate >= firstDayOfMonth)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

            var activeStudents = await _context.Students.CountAsync();
            var openTabs = await _context.Tabs.CountAsync(t => t.Status == "Open");
            var lowStock = await _context.Products.CountAsync(p => p.IsActive && p.Stock < 5);
            var reservationsToday = await _context.Reservations.CountAsync(r => r.ReservationDate >= today && r.ReservationDate < today.AddDays(1));
            var cashRegisterOpen = await _context.CashRegisters.AnyAsync(cr => cr.Status == "Open");

            return new DashboardSummaryDto
            {
                SalesToday = salesToday,
                SalesMonth = salesMonth,
                ActiveStudents = activeStudents,
                OpenTabs = openTabs,
                LowStockProducts = lowStock,
                TotalReservationsToday = reservationsToday,
                CashRegisterOpen = cashRegisterOpen
            };
        }

        public async Task<IEnumerable<object>> GetSalesByDayAsync(int days)
        {
            var from = DateTime.Now.Date.AddDays(-days + 1);

            var grouped = await _context.Sales
                .Where(s => s.SaleDate >= from)
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Total = g.Sum(s => s.TotalAmount)
                })
                .ToListAsync();

            return grouped
                .Select(g => new SalesByDayDto
                {
                    Date = g.Date.ToString("yyyy-MM-dd"),
                    Total = g.Total
                })
                .OrderBy(x => x.Date);
        }

        public async Task<IEnumerable<object>> GetSalesByCategoryAsync()
        {
            return await _context.SaleItems
                .Include(si => si.Product).ThenInclude(p => p!.Category)
                .GroupBy(si => si.Product != null && si.Product.Category != null
                    ? si.Product.Category.Name
                    : "Sem Categoria")
                .Select(g => new SalesByCategoryDto
                {
                    Category = g.Key,
                    Total = g.Sum(si => si.Quantity * si.UnitPrice)
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetTopProductsAsync(int limit)
        {
            return await _context.SaleItems
                .Include(si => si.Product)
                .GroupBy(si => new { si.ProductId, Name = si.Product != null ? si.Product.Name : "Desconhecido" })
                .Select(g => new TopProductDto
                {
                    ProductName = g.Key.Name,
                    TotalQty = g.Sum(si => si.Quantity),
                    TotalRevenue = g.Sum(si => si.Quantity * si.UnitPrice)
                })
                .OrderByDescending(x => x.TotalQty)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetSalesByProductAsync(int days)
        {
            var from = DateTime.Now.Date.AddDays(-days + 1);

            return await _context.SaleItems
                .Include(si => si.Sale)
                .Include(si => si.Product)
                .Where(si => si.Sale != null && si.Sale.SaleDate >= from)
                .GroupBy(si => new { si.ProductId, Name = si.Product != null ? si.Product.Name : "Desconhecido" })
                .Select(g => new
                {
                    ProductName = g.Key.Name,
                    TotalQty = g.Sum(si => si.Quantity),
                    TotalRevenue = g.Sum(si => si.Quantity * si.UnitPrice)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetInventorySummaryAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    CategoryName = p.Category != null ? p.Category.Name : "Sem Categoria",
                    p.Stock,
                    p.Price,
                    p.CostPrice,
                    IsLowStock = p.Stock < 5
                })
                .OrderBy(p => p.Stock)
                .ToListAsync();
        }
    }
}
