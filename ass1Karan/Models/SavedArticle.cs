using System;
using System.ComponentModel.DataAnnotations;

namespace ass1Karan.Models
{
    public class SavedArticle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }

        public string ImageUrl { get; set; }

        public DateTime SavedAt { get; set; } = DateTime.Now;

        public string UserEmail { get; set; }
    }
}
