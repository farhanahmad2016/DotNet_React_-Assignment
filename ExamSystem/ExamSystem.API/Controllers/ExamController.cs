using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ExamSystem.API.Models.DTOs;
using ExamSystem.API.Services;
using ExamSystem.API.Validators;
using ExamSystem.API.Extensions;

namespace ExamSystem.API.Controllers
{
    /// <summary>
    /// Controller for managing exam operations including creation, updates, and retrieval
    /// Handles admin exam management and student exam access
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All endpoints require authentication
    public class ExamController : ControllerBase
    {
        private readonly IExamService _examService;
        private readonly CreateExamDtoValidator _examValidator;

        /// <summary>
        /// Initializes the ExamController with required services and validators
        /// </summary>
        /// <param name="examService">Service for exam operations</param>
        /// <param name="examValidator">FluentValidation validator for exam creation/updates</param>
        public ExamController(IExamService examService, CreateExamDtoValidator examValidator)
        {
            _examService = examService;
            _examValidator = examValidator;
        }

        /// <summary>
        /// Creates a new exam in the system
        /// Validates exam data using FluentValidation before creation
        /// </summary>
        /// <param name="examDto">Exam creation data including title, max attempts, and cooldown</param>
        /// <returns>Created exam details if successful, validation errors if invalid</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only admins can create exams
        public async Task<IActionResult> CreateExam([FromBody] CreateExamDto examDto)
        {
            // Validate exam data using FluentValidation
            var validationResult = await this.ValidateAsync(examDto, _examValidator);
            if (validationResult != null) return validationResult;

            try
            {
                var exam = await _examService.CreateOrUpdateExamAsync(examDto);
                if (exam == null)
                    return BadRequest("Failed to create exam");

                return Ok(exam);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while creating the exam");
            }
        }

        /// <summary>
        /// Updates an existing exam in the system
        /// Clears all existing attempts when exam is updated to maintain data integrity
        /// </summary>
        /// <param name="examId">ID of the exam to update</param>
        /// <param name="examDto">Updated exam data</param>
        /// <returns>Updated exam details if successful, not found if exam doesn't exist</returns>
        [HttpPut("{examId}")]
        [Authorize(Roles = "Admin")] // Only admins can update exams
        public async Task<IActionResult> UpdateExam(Guid examId, [FromBody] CreateExamDto examDto)
        {
            // Validate exam data using FluentValidation
            var validationResult = await this.ValidateAsync(examDto, _examValidator);
            if (validationResult != null) return validationResult;

            try
            {
                // Update exam and clear existing attempts
                var exam = await _examService.CreateOrUpdateExamAsync(examDto, examId);
                if (exam == null)
                    return NotFound("Exam not found");

                return Ok(exam);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating the exam");
            }
        }

        /// <summary>
        /// Retrieves the most recent exam for the authenticated student
        /// Includes student-specific information like remaining attempts and cooldown status
        /// Legacy endpoint - use /api/attempt/exams for multiple exam support
        /// </summary>
        /// <returns>Most recent exam with student-specific attempt information</returns>
        [HttpGet("student")]
        [Authorize(Roles = "Student")] // Only students can access exam information
        public async Task<IActionResult> GetExamForStudent()
        {
            try
            {
                // Extract student ID from JWT token claims
                var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var exam = await _examService.GetExamForStudentAsync(studentId);

                if (exam == null)
                    return NotFound("No exam found");

                return Ok(exam);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving the exam");
            }
        }
    }
}