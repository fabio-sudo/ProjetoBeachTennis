namespace ArenaBackend.DTOs
{
    public class OpenCashRegisterDto
    {
        public decimal OpeningAmount { get; set; }
    }

    public class CloseCashRegisterDto
    {
        public decimal ClosingAmount { get; set; }
    }

    public class AddCashTransactionDto
    {
        public string Type { get; set; } = "Expense"; // Sale | Expense | Adjustment
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
}
