using System.ComponentModel.DataAnnotations;

namespace ass1Karan.Models
{
    public class LocationEntry
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Tag { get; set; }

        public string Phone { get; set; }
        public string DirectionsUrl { get; set; }
    }
}
