using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArenaBackend.Models
{
    public class Sale
    {
        [Key]
        public int Id { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.Now;

        public string PaymentType { get; set; } = "Dinheiro"; // Default

        public string Status { get; set; } = "Completed";

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalAmount { get; set; }

        public int? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        public int? StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    }
}
