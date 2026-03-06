using System.ComponentModel.DataAnnotations;

namespace ArenaBackend.DTOs
{
    public class CancelActionDto
    {
        [Required]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public string CancelledBy { get; set; } = string.Empty;
    }
}
