using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using JobApplicationTracker.Database;
using JobApplicationTracker.Models;

namespace JobApplicationTrackerTests.Repositories
{
    public abstract class DatabaseUtil : IDisposable
    {
        private readonly SqliteConnection _connection;
        protected readonly ApplicationDbContext DbContext;

        protected DatabaseUtil()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            DbContext = new ApplicationDbContext(options);
            DbContext.Database.EnsureCreated();
        }

        public void Dispose()
        {
            DbContext.Database.EnsureDeleted();
            DbContext.Dispose();
            _connection.Dispose();
        }

        protected async Task<User> CreateTestUser(string email = "test@example.com")
        {
            var user = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                FirstName = "Test",
                LastName = "User",
                CreatedAt = DateTime.UtcNow
            };

            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();
            return user;
        }

        protected async Task<JobApplication> CreateTestApplication(int userId, string companyName = "Test Company")
        {
            var application = new JobApplication
            {
                CompanyName = companyName,
                Position = "Developer",
                Status = "Applied",
                DateApplied = DateTime.UtcNow,
                UserId = userId
            };

            DbContext.Applications.Add(application);
            await DbContext.SaveChangesAsync();
            return application;
        }
    }
}