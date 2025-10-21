using System.ComponentModel.DataAnnotations;

namespace WebService.Models
{
    public class ClientInfo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string IPAddress { get; set; }

        [Required]
        public int Port { get; set; }

        public string? DisplayName { get; set; }

        public int JobsCompleted { get; set; } = 0;

        public DateTime RegisteredAt { get; set; } = new DateTime(2025, 10, 5, 3, 44, 52, 867);
    }
}
