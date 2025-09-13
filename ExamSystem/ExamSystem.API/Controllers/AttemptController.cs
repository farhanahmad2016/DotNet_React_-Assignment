using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ExamSystem.API.Services;

namespace ExamSystem.API.Controllers
{
    /// <summary>
    /// Controller for managing exam attempts and exam access for students and admins
    /// Handles starting attempts, submitting attempts, and retrieving attempt history
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All endpoints require authentication
    public class AttemptController : ControllerBase
    {
        private readonly IExamService _examService;

        /// <summary>
        /// Initializes the AttemptController with required services
        /// </summary>
        /// <param name="examService">Service for exam and attempt operations</param>
        public AttemptController(IExamService examService)
        {
            _examService = examService;
        }

        /// <summary>
        /// Retrieves all attempts made by the authenticated student
        /// Returns attempt history with exam titles and status information
        /// </summary>
        /// <returns>List of student's attempts with details</returns>
        [HttpGet("student")]
        [Authorize(Roles = "Student")] // Only students can access their own attempts
        public async Task<IActionResult> GetStudentAttempts()
        {
            try
            {
                // Extract student ID from JWT token claims
                var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var attempts = await _examService.GetStudentAttemptsAsync(studentId);
                return Ok(attempts);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving attempts");
            }
        }

        /// <summary>
        /// Retrieves all available exams for the authenticated student
        /// Includes remaining attempts and cooldown information for each exam
        /// </summary>
        /// <returns>List of exams with student-specific attempt information</returns>
        [HttpGet("exams")]
        [Authorize(Roles = "Student")] // Only students can view available exams
        public async Task<IActionResult> GetAllExamsForStudent()
        {
            try
            {
                // Extract student ID from JWT token claims
                var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var exams = await _examService.GetAllExamsForStudentAsync(studentId);
                return Ok(exams);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving exams");
            }
        }

        /// <summary>
        /// Retrieves all attempts from all students across all exams
        /// Admin-only endpoint for monitoring and analytics
        /// </summary>
        /// <returns>Complete list of all attempts in the system</returns>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")] // Only admins can view all attempts
        public async Task<IActionResult> GetAllAttempts()
        {
            try
            {
                var attempts = await _examService.GetAllAttemptsAsync();
                return Ok(attempts);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving all attempts");
            }
        }

        /// <summary>
        /// Retrieves all exams in the system for admin management
        /// Provides complete exam information without student-specific data
        /// </summary>
        /// <returns>List of all exams in the system</returns>
        [HttpGet("admin/exams")]
        [Authorize(Roles = "Admin")] // Only admins can view all exams
        public async Task<IActionResult> GetAllExamsForAdmin()
        {
            try
            {
                var exams = await _examService.GetAllExamsAsync();
                return Ok(exams);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving exams");
            }
        }

        /// <summary>
        /// Starts a new exam attempt for the authenticated student
        /// Validates cooldown periods, maximum attempts, and existing in-progress attempts
        /// </summary>
        /// <param name="examId">ID of the exam to attempt</param>
        /// <returns>New attempt details if successful, error if validation fails</returns>
        [HttpPost("start/{examId}")]
        [Authorize(Roles = "Student")] // Only students can start attempts
        public async Task<IActionResult> StartAttempt(Guid examId)
        {
            try
            {
                // Extract student ID from JWT token claims
                var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var attempt = await _examService.StartAttemptAsync(studentId, examId);

                // Return error if attempt cannot be started (cooldown, max attempts, etc.)
                if (attempt == null)
                    return BadRequest("Cannot start attempt. Check cooldown or max attempts.");

                return Ok(attempt);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while starting the attempt: {ex.Message}");
            }
        }

        /// <summary>
        /// Submits an in-progress exam attempt for the authenticated student
        /// Marks the attempt as completed and records the end time
        /// </summary>
        /// <param name="attemptId">ID of the attempt to submit</param>
        /// <returns>Updated attempt details if successful, error if attempt not found or already completed</returns>
        [HttpPost("{attemptId}/submit")]
        [Authorize(Roles = "Student")] // Only students can submit their own attempts
        public async Task<IActionResult> SubmitAttempt(Guid attemptId)
        {
            try
            {
                // Extract student ID from JWT token claims for security validation
                var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var attempt = await _examService.SubmitAttemptAsync(attemptId, studentId);

                // Return error if attempt cannot be submitted (not found, already completed, etc.)
                if (attempt == null)
                    return BadRequest("Cannot submit attempt");

                return Ok(attempt);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while submitting the attempt");
            }
        }
    }
}