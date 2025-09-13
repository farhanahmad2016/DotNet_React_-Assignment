using ExamSystem.API.Models;
using ExamSystem.API.Models.DTOs;

namespace ExamSystem.API.Services
{
    /// <summary>
    /// Service for handling authentication operations
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates user credentials and returns JWT token
        /// </summary>
        /// <param name="loginDto">User login credentials</param>
        /// <returns>JWT token if valid, null if invalid</returns>
        Task<string?> LoginAsync(LoginDto loginDto);
        
        /// <summary>
        /// Retrieves user by ID
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>User if found, null otherwise</returns>
        Task<User?> GetUserByIdAsync(Guid userId);
        
        /// <summary>
        /// Generates JWT token for authenticated user
        /// </summary>
        /// <param name="user">User to generate token for</param>
        /// <returns>JWT token string</returns>
        string GenerateJwtToken(User user);
    }
}
