namespace JobApplicationTracker.Database
{
    using JobApplicationTracker.Models;
    using Microsoft.EntityFrameworkCore;

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<JobApplication> Applications { get; set; }

        public DbSet<User> Users { get; set; }
    }
}
