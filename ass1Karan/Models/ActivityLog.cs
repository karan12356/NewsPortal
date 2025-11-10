using System;
using System.ComponentModel.DataAnnotations;

namespace ass1Karan.Models
{
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Action { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
