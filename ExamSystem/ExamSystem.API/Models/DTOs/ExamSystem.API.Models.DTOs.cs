using System.ComponentModel.DataAnnotations;

namespace ExamSystem.API.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object for user authentication requests
    /// Contains credentials required for login validation
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Username for authentication
        /// Must be 3-50 characters, containing only letters, numbers, and underscores
        /// </summary>
        public string Username { get; set; } = string.Empty;
       
        /// <summary>
        /// Password for authentication
        /// Must be at least 6 characters long
        /// Validated against BCrypt hash stored in database
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}
