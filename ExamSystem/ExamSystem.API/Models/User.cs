namespace ExamSystem.API.Models
{
    /// <summary>
    /// Entity representing a user in the authentication system
    /// Stores login credentials and role information for access control
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique identifier for the user
        /// Used in JWT tokens for authentication
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Unique username for login authentication
        /// Must be unique across the system
        /// </summary>
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// BCrypt hashed password for secure authentication
        /// Never store plain text passwords
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;
        
        /// <summary>
        /// User role for authorization
        /// Valid values: "Admin" (can create/manage exams) or "Student" (can take exams)
        /// </summary>
        public string Role { get; set; } = string.Empty;
        
        /// <summary>
        /// Timestamp when the user account was created
        /// Used for auditing and account management
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}