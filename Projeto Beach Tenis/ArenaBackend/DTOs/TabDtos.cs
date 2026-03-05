namespace ArenaBackend.DTOs
{
    public class CreateTabDto
    {
        public int? CustomerId { get; set; }
        public int? StudentId { get; set; }
        public string? TableNumber { get; set; }
    }

    public class AddTabItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CloseTabDto
    {
        public string PaymentType { get; set; } = "Dinheiro";
    }
}
