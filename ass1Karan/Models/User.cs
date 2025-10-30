using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ass1Karan.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, EmailAddress]
        [Column(TypeName = "varchar(150)")]
        public string Email { get; set; }

        [Required]
        [Column(TypeName = "varchar(255)")]
        public string PasswordHash { get; set; }
    }
}
