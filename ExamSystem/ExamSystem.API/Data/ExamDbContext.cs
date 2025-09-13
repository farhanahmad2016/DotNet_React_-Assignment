using Microsoft.EntityFrameworkCore;
using ExamSystem.API.Models;

namespace ExamSystem.API.Data
{
    /// <summary>
    /// Entity Framework database context for the Exam System
    /// Configures entity relationships, constraints, and provides seed data
    /// </summary>
    public class ExamDbContext : DbContext
    {
        /// <summary>
        /// Initializes the database context with configuration options
        /// </summary>
        /// <param name="options">Entity Framework configuration options</param>
        public ExamDbContext(DbContextOptions<ExamDbContext> options) : base(options) { }

        /// <summary>
        /// DbSet for User entities - handles authentication and authorization
        /// </summary>
        public DbSet<User> Users { get; set; }
        
        /// <summary>
        /// DbSet for Exam entities - stores exam configurations and settings
        /// </summary>
        public DbSet<Exam> Exams { get; set; }
        
        /// <summary>
        /// DbSet for Attempt entities - tracks student exam attempts and progress
        /// </summary>
        public DbSet<Attempt> Attempts { get; set; }

        /// <summary>
        /// Configures entity relationships, constraints, and database schema
        /// Sets up foreign keys and seeds initial data
        /// </summary>
        /// <param name="modelBuilder">Entity Framework model builder</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Attempt -> Exam relationship (required)
            modelBuilder.Entity<Attempt>()
                .HasOne(a => a.Exam)
                .WithMany(e => e.Attempts)
                .HasForeignKey(a => a.ExamId);

            // Note: Attempt -> Student relationship is intentionally not configured
            // This allows JWT-based student IDs that may not exist in the Users table
            // Students can take exams without requiring database user records
            
            // Initialize database with default admin and student accounts
            SeedData(modelBuilder);
        }

        /// <summary>
        /// Seeds the database with initial user accounts for testing and development
        /// Creates default admin and student accounts with secure password hashing
        /// </summary>
        /// <param name="modelBuilder">Entity Framework model builder for data seeding</param>
        private void SeedData(ModelBuilder modelBuilder)
        {
            // Generate unique IDs for seed users
            var adminId = Guid.NewGuid();
            var studentId = Guid.NewGuid();

            // Seed default user accounts with BCrypt password hashing
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = adminId,
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), // Secure password hashing
                    Role = "Admin" // Can create and manage exams
                },
                new User
                {
                    Id = studentId,
                    Username = "student",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("student123"), // Secure password hashing
                    Role = "Student" // Can take exams and view attempts
                }
            );
        }
    }
}