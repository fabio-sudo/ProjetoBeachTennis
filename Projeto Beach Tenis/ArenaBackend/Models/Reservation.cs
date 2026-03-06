using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArenaBackend.Models
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CourtId { get; set; }

        [ForeignKey("CourtId")]
        public Court? Court { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "date")]
        public DateTime ReservationDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Agendado"; // Agendado, Finalizado, Cancelado

        [StringLength(50)]
        public string? PaymentType { get; set; } // Dinheiro, PIX, Cartão Crédito, Cartão Débito

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
