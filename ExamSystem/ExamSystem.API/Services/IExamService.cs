using ExamSystem.API.Models;
using ExamSystem.API.Models.DTOs;

namespace ExamSystem.API.Services
{
    /// <summary>
    /// Service interface for exam and attempt management operations
    /// Defines contracts for exam creation, attempt tracking, and validation logic
    /// </summary>
    public interface IExamService
    {
        /// <summary>
        /// Creates a new exam or updates an existing one
        /// When updating, clears all existing attempts to maintain data integrity
        /// </summary>
        /// <param name="examDto">Exam data for creation or update</param>
        /// <param name="examId">Optional exam ID for updates, null for creation</param>
        /// <returns>Created or updated exam entity, null if exam not found for update</returns>
        Task<Exam?> CreateOrUpdateExamAsync(CreateExamDto examDto, Guid? examId = null);
        
        /// <summary>
        /// Retrieves the most recent exam for a specific student with attempt information
        /// Legacy method - use GetAllExamsForStudentAsync for multiple exam support
        /// </summary>
        /// <param name="studentId">ID of the student requesting exam information</param>
        /// <returns>Exam details with student-specific attempt information, null if no exams exist</returns>
        Task<ExamResponseDto?> GetExamForStudentAsync(Guid studentId);
        
        /// <summary>
        /// Retrieves all attempts made by a specific student across all exams
        /// </summary>
        /// <param name="studentId">ID of the student whose attempts to retrieve</param>
        /// <returns>List of all attempts made by the student with exam information</returns>
        Task<List<AttemptResponseDto>> GetStudentAttemptsAsync(Guid studentId);
        
        /// <summary>
        /// Retrieves all attempts from all students across all exams (admin only)
        /// </summary>
        /// <returns>Complete list of all attempts in the system with exam information</returns>
        Task<List<AttemptResponseDto>> GetAllAttemptsAsync();
        
        /// <summary>
        /// Starts a new exam attempt with comprehensive validation
        /// Validates maximum attempts, cooldown periods, and existing in-progress attempts
        /// </summary>
        /// <param name="studentId">ID of the student starting the attempt</param>
        /// <param name="examId">Optional specific exam ID, uses most recent exam if null</param>
        /// <returns>New or existing attempt details if successful, null if validation fails</returns>
        Task<AttemptResponseDto?> StartAttemptAsync(Guid studentId, Guid? examId = null);
        
        /// <summary>
        /// Retrieves all exams with student-specific attempt information
        /// Supports multiple exam functionality with independent attempt tracking
        /// </summary>
        /// <param name="studentId">ID of the student requesting exam information</param>
        /// <returns>List of all exams with student-specific attempt data for each</returns>
        Task<List<ExamResponseDto>> GetAllExamsForStudentAsync(Guid studentId);
        
        /// <summary>
        /// Retrieves all exams without student-specific information (admin only)
        /// </summary>
        /// <returns>List of all exams with basic information</returns>
        Task<List<ExamResponseDto>> GetAllExamsAsync();
        
        /// <summary>
        /// Submits an in-progress exam attempt, marking it as completed
        /// Validates attempt ownership and status before submission
        /// </summary>
        /// <param name="attemptId">ID of the attempt to submit</param>
        /// <param name="studentId">ID of the student submitting (for security validation)</param>
        /// <returns>Updated attempt details if successful, null if attempt not found or invalid</returns>
        Task<AttemptResponseDto?> SubmitAttemptAsync(Guid attemptId, Guid studentId);
    }
}