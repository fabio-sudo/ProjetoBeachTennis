using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArenaBackend.Models
{
    public class Tab
    {
        [Key]
        public int Id { get; set; }

        public int? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        public int? StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        public string? TableNumber { get; set; }

        public DateTime OpenedAt { get; set; } = DateTime.Now;
        public DateTime? ClosedAt { get; set; }

        public string Status { get; set; } = "Open"; // Open | Closed

        public int? SaleId { get; set; }
        [ForeignKey("SaleId")]
        public Sale? Sale { get; set; }

        public ICollection<TabItem> Items { get; set; } = new List<TabItem>();
    }
}
