namespace ArenaBackend.DTOs
{
    public class CreatePurchaseOrderDto
    {
        public int SupplierId { get; set; }
        public string? Notes { get; set; }
        public List<PurchaseOrderItemDto> Items { get; set; } = new();
    }

    public class PurchaseOrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal CostPrice { get; set; }
    }
}
