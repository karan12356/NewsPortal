using System;
using System.Collections.Generic;
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
        public List<Comment> Comments { get; set; } = new();
        public List<Rating> Ratings { get; set; } = new();
    }

    public class Comment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(250, MinimumLength = 5)]
        public string Content { get; set; }
        public string AuthorEmail { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int SavedArticleId { get; set; }
        public SavedArticle SavedArticle { get; set; }
    }

    public class Rating
    {
        [Key]
        public int Id { get; set; }
        [Range(1, 5)]
        public int Value { get; set; }
        public string UserEmail { get; set; }
        public int SavedArticleId { get; set; }
        public SavedArticle SavedArticle { get; set; }
    }
}
