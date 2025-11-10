using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ass1Karan.Models
{
    public class UserProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } 

        [StringLength(100)]
        public string FullName { get; set; }

        public string? ProfileImageUrl { get; set; } = "/images/default-avatar.png";

        public DateTime JoinedDate { get; set; } = DateTime.Now;
    }
}
