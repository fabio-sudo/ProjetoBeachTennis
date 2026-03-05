using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArenaBackend.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }

        public int Stock { get; set; } = 0;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal CostPrice { get; set; } = 0;

        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public ProductCategory? Category { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
