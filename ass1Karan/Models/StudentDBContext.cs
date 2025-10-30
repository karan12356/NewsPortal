using Microsoft.EntityFrameworkCore;

namespace ass1Karan.Models
{
    public class StudentDBContext : DbContext
    {
        public StudentDBContext(DbContextOptions options) : base(options)
        {

        }
       
        public DbSet<User> Users { get; set; }
        public DbSet<SavedArticle> SavedArticles { get; set; }

    }
}
