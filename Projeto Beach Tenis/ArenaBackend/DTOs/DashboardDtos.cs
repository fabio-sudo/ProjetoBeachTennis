namespace ArenaBackend.DTOs
{
    public class DashboardSummaryDto
    {
        public decimal SalesToday { get; set; }
        public decimal SalesMonth { get; set; }
        public int ActiveStudents { get; set; }
        public int OpenTabs { get; set; }
        public int LowStockProducts { get; set; }
        public int TotalReservationsToday { get; set; }
        public bool CashRegisterOpen { get; set; }
    }

    public class SalesByDayDto
    {
        public string Date { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class SalesByCategoryDto
    {
        public string Category { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class TopProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalQty { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
