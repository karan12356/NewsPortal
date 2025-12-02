using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ass1Karan.Models
{
    public class StudentDBContext : IdentityDbContext<IdentityUser>
    {
        public StudentDBContext(DbContextOptions options) : base(options)
        {

        }
       
        public DbSet<User> Users { get; set; }
        public DbSet<SavedArticle> SavedArticles { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<PpsResult> PpsResults { get; set; }
        public DbSet<LocationEntry> LocationEntries { get; set; }
        public DbSet<TagImage> TagImages { get; set; }


    }
}
