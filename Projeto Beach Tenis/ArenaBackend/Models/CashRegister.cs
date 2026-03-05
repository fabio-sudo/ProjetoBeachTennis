using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArenaBackend.Models
{
    public class CashRegister
    {
        [Key]
        public int Id { get; set; }

        public DateTime OpenedAt { get; set; } = DateTime.Now;
        public DateTime? ClosedAt { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal OpeningAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? ClosingAmount { get; set; }

        public string Status { get; set; } = "Open"; // Open | Closed

        public ICollection<CashTransaction> Transactions { get; set; } = new List<CashTransaction>();
    }
}
