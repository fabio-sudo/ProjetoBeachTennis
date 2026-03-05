using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArenaBackend.Models
{
    public class CashTransaction
    {
        [Key]
        public int Id { get; set; }

        public int CashRegisterId { get; set; }
        [ForeignKey("CashRegisterId")]
        public CashRegister? CashRegister { get; set; }

        /// <summary>Sale | Expense | Adjustment</summary>
        public string Type { get; set; } = "Sale";

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        public string? Description { get; set; }

        public int? SaleId { get; set; }
        [ForeignKey("SaleId")]
        public Sale? Sale { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
