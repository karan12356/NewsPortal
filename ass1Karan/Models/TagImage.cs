using System.ComponentModel.DataAnnotations;

namespace ass1Karan.Models
{
    public class TagImage
    {
        public int Id { get; set; }

        [Required]
        public string Tag { get; set; } 

        public string ImageName { get; set; } 
    }
}
