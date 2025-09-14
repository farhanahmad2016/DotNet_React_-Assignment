using Microsoft.EntityFrameworkCore;
using ExamSystem.API.Data;
using ExamSystem.API.Models;
using ExamSystem.API.Models.DTOs;

namespace ExamSystem.API.Services
{
    /// <summary>
    /// Service implementation for exam and attempt management
    /// Handles all business logic for exam operations, attempt tracking, and validation
    /// </summary>
    public class ExamService : IExamService
    {
        private readonly ExamDbContext _context;

        /// <summary>
        /// Initializes the ExamService with database context
        /// </summary>
        /// <param name="context">Entity Framework database context</param>
        public ExamService(ExamDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new exam or updates an existing one
        /// When updating, all existing attempts are cleared to maintain data integrity
        /// </summary>
        /// <param name="examDto">Exam data for creation or update</param>
        /// <param name="examId">Optional exam ID for updates, null for creation</param>
        /// <returns>Created or updated exam entity, null if exam not found for update</returns>
        public async Task<Exam?> CreateOrUpdateExamAsync(CreateExamDto examDto, Guid? examId = null)
        {
            Exam? exam;

            if (examId.HasValue)
            {
                // Update existing exam
                exam = await _context.Exams.FindAsync(examId.Value);
                if (exam == null) return null;

                // Clear all attempts when exam is updated to prevent inconsistent data
                var attempts = await _context.Attempts.Where(a => a.ExamId == examId.Value).ToListAsync();
                _context.Attempts.RemoveRange(attempts);

                // Update exam properties
                exam.Title = examDto.Title;
                exam.MaxAttempts = examDto.MaxAttempts;
                exam.CooldownMinutes = examDto.CooldownMinutes;
                exam.LastModified = DateTime.UtcNow;
            }
            else
            {
                // Create new exam
                exam = new Exam
                {
                    ExamId = Guid.NewGuid(),
                    Title = examDto.Title,
                    MaxAttempts = examDto.MaxAttempts,
                    CooldownMinutes = examDto.CooldownMinutes
                };
                _context.Exams.Add(exam);
            }

            await _context.SaveChangesAsync();
            return exam;
        }

        /// <summary>
        /// Retrieves the most recent exam for a specific student
        /// Calculates remaining attempts and cooldown information based on student's attempt history
        /// Legacy method - use GetAllExamsForStudentAsync for multiple exam support
        /// </summary>
        /// <param name="studentId">ID of the student requesting exam information</param>
        /// <returns>Exam details with student-specific attempt information, null if no exams exist</returns>
        public async Task<ExamResponseDto?> GetExamForStudentAsync(Guid studentId)
        {
            // Get the most recently modified exam
            var exam = await _context.Exams.OrderByDescending(e => e.LastModified).FirstOrDefaultAsync();
            if (exam == null) return null;

            // Get all attempts by this student for this exam
            var attempts = await _context.Attempts
                .Where(a => a.ExamId == exam.ExamId && a.StudentId == studentId)
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();

            // Calculate remaining attempts
            var remainingAttempts = Math.Max(0, exam.MaxAttempts - attempts.Count);
            DateTime? nextAttemptAvailableAt = null;

            // Check if student is in cooldown period
            if (attempts.Any() && exam.CooldownMinutes > 0)
            {
                var lastAttempt = attempts.First();
                // Cooldown starts from StartTime (when attempt began)
                var cooldownEnd = lastAttempt.StartTime.AddMinutes(exam.CooldownMinutes);
                if (cooldownEnd > DateTime.UtcNow)
                {
                    nextAttemptAvailableAt = cooldownEnd;
                }
            }

            return new ExamResponseDto
            {
                ExamId = exam.ExamId,
                Title = exam.Title,
                MaxAttempts = exam.MaxAttempts,
                CooldownMinutes = exam.CooldownMinutes,
                LastModified = exam.LastModified,
                RemainingAttempts = remainingAttempts,
                NextAttemptAvailableAt = nextAttemptAvailableAt
            };
        }

        /// <summary>
        /// Retrieves all attempts made by a specific student across all exams
        /// Includes exam titles for better user experience
        /// </summary>
        /// <param name="studentId">ID of the student whose attempts to retrieve</param>
        /// <returns>List of all attempts made by the student with exam information</returns>
        public async Task<List<AttemptResponseDto>> GetStudentAttemptsAsync(Guid studentId)
        {
            // Get all attempts by the student, including exam information
            var attempts = await _context.Attempts
                .Include(a => a.Exam) // Include exam data for titles
                .Where(a => a.StudentId == studentId)
                .OrderBy(a => a.AttemptNumber)
                .ToListAsync();

            // Map to DTOs with exam titles
            return attempts.Select(a => new AttemptResponseDto
            {
                AttemptId = a.AttemptId,
                AttemptNumber = a.AttemptNumber,
                AttemptStatus = a.AttemptStatus,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                ExamTitle = a.Exam?.Title ?? "Unknown Exam" // Fallback for missing exam data
            }).ToList();
        }

        /// <summary>
        /// Retrieves all attempts from all students across all exams
        /// Admin-only functionality for monitoring and analytics
        /// </summary>
        /// <returns>Complete list of all attempts in the system with exam information</returns>
        public async Task<List<AttemptResponseDto>> GetAllAttemptsAsync()
        {
            // Get all attempts from all students, including exam information
            var attempts = await _context.Attempts
                .Include(a => a.Exam) // Include exam data for titles
                .OrderBy(a => a.AttemptNumber)
                .ToListAsync();

            // Map to DTOs with exam titles
            return attempts.Select(a => new AttemptResponseDto
            {
                AttemptId = a.AttemptId,
                AttemptNumber = a.AttemptNumber,
                AttemptStatus = a.AttemptStatus,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                ExamTitle = a.Exam?.Title ?? "Unknown Exam" // Fallback for missing exam data
            }).ToList();
        }

        /// <summary>
        /// Retrieves all exams in the system without student-specific information
        /// Used by admin interface for exam management
        /// </summary>
        /// <returns>List of all exams with basic information (no student-specific data)</returns>
        public async Task<List<ExamResponseDto>> GetAllExamsAsync()
        {
            // Get all exams ordered by most recent first
            var exams = await _context.Exams.OrderByDescending(e => e.LastModified).ToListAsync();
            
            // Map to DTOs without student-specific information
            return exams.Select(e => new ExamResponseDto
            {
                ExamId = e.ExamId,
                Title = e.Title,
                MaxAttempts = e.MaxAttempts,
                CooldownMinutes = e.CooldownMinutes,
                LastModified = e.LastModified,
                RemainingAttempts = 0, // Not applicable for admin view
                NextAttemptAvailableAt = null // Not applicable for admin view
            }).ToList();
        }

        /// <summary>
        /// Starts a new exam attempt for a student with comprehensive validation
        /// Validates maximum attempts, cooldown periods, and existing in-progress attempts
        /// </summary>
        /// <param name="studentId">ID of the student starting the attempt</param>
        /// <param name="examId">Optional specific exam ID, uses most recent exam if null</param>
        /// <returns>New or existing attempt details if successful, null if validation fails</returns>
        public async Task<AttemptResponseDto?> StartAttemptAsync(Guid studentId, Guid? examId = null)
        {
            // Get specific exam or most recent exam if no ID provided
            var exam = examId.HasValue 
                ? await _context.Exams.FindAsync(examId.Value)
                : await _context.Exams.OrderByDescending(e => e.LastModified).FirstOrDefaultAsync();
            if (exam == null) return null;

            // Get all existing attempts by this student for this exam
            var existingAttempts = await _context.Attempts
                .Where(a => a.ExamId == exam.ExamId && a.StudentId == studentId)
                .ToListAsync();

            // Validation 1: Check if maximum attempts reached
            if (existingAttempts.Count >= exam.MaxAttempts) return null;

            // Validation 2: Check cooldown period
            if (existingAttempts.Any() && exam.CooldownMinutes > 0)
            {
                var lastAttempt = existingAttempts.OrderByDescending(a => a.StartTime).First();
                // Cooldown starts from StartTime (when attempt began)
                var cooldownEnd = lastAttempt.StartTime.AddMinutes(exam.CooldownMinutes);
                if (cooldownEnd > DateTime.UtcNow) return null; // Still in cooldown
            }

            // Validation 3: Check if there's already an in-progress attempt
            var inProgressAttempt = existingAttempts.FirstOrDefault(a => a.AttemptStatus == "InProgress");
            if (inProgressAttempt != null)
            {
                // Return existing in-progress attempt instead of creating new one
                return new AttemptResponseDto
                {
                    AttemptId = inProgressAttempt.AttemptId,
                    AttemptNumber = inProgressAttempt.AttemptNumber,
                    AttemptStatus = inProgressAttempt.AttemptStatus,
                    StartTime = inProgressAttempt.StartTime,
                    EndTime = inProgressAttempt.EndTime
                };
            }

            // Create new attempt
            var attempt = new Attempt
            {
                AttemptId = Guid.NewGuid(),
                ExamId = exam.ExamId,
                StudentId = studentId,
                AttemptNumber = existingAttempts.Count + 1, // Sequential numbering
                AttemptStatus = "InProgress",
                StartTime = DateTime.UtcNow
            };

            _context.Attempts.Add(attempt);
            await _context.SaveChangesAsync();
            
            return new AttemptResponseDto
            {
                AttemptId = attempt.AttemptId,
                AttemptNumber = attempt.AttemptNumber,
                AttemptStatus = attempt.AttemptStatus,
                StartTime = attempt.StartTime,
                EndTime = attempt.EndTime
            };
        }

        /// <summary>
        /// Submits an in-progress exam attempt, marking it as completed
        /// Validates that the attempt belongs to the student and is in progress
        /// </summary>
        /// <param name="attemptId">ID of the attempt to submit</param>
        /// <param name="studentId">ID of the student submitting (for security validation)</param>
        /// <returns>Updated attempt details if successful, null if attempt not found or invalid</returns>
        public async Task<AttemptResponseDto?> SubmitAttemptAsync(Guid attemptId, Guid studentId)
        {
            // Find attempt with security validation (must belong to the student)
            var attempt = await _context.Attempts
                .FirstOrDefaultAsync(a => a.AttemptId == attemptId && a.StudentId == studentId);

            // Validate attempt exists and is in progress
            if (attempt == null || attempt.AttemptStatus != "InProgress") return null;

            // Mark attempt as completed and record end time
            attempt.AttemptStatus = "Completed";
            attempt.EndTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            return new AttemptResponseDto
            {
                AttemptId = attempt.AttemptId,
                AttemptNumber = attempt.AttemptNumber,
                AttemptStatus = attempt.AttemptStatus,
                StartTime = attempt.StartTime,
                EndTime = attempt.EndTime
            };
        }

        /// <summary>
        /// Retrieves all exams with student-specific attempt information
        /// Calculates remaining attempts and cooldown status for each exam independently
        /// Supports multiple exam functionality where students can attempt each exam separately
        /// </summary>
        /// <param name="studentId">ID of the student requesting exam information</param>
        /// <returns>List of all exams with student-specific attempt data for each</returns>
        public async Task<List<ExamResponseDto>> GetAllExamsForStudentAsync(Guid studentId)
        {
            // Get all exams ordered by most recent first
            var exams = await _context.Exams.OrderByDescending(e => e.LastModified).ToListAsync();
            var result = new List<ExamResponseDto>();

            // Process each exam independently to calculate student-specific data
            foreach (var exam in exams)
            {
                // Get attempts for this specific exam and student
                var attempts = await _context.Attempts
                    .Where(a => a.ExamId == exam.ExamId && a.StudentId == studentId)
                    .ToListAsync();

                // Calculate remaining attempts for this exam
                var remainingAttempts = Math.Max(0, exam.MaxAttempts - attempts.Count);
                DateTime? nextAttemptAvailableAt = null;

                // Check cooldown status for this exam
                if (attempts.Any() && exam.CooldownMinutes > 0)
                {
                    var lastAttempt = attempts.OrderByDescending(a => a.StartTime).First();
                    // Cooldown starts from StartTime (when attempt began)
                    var cooldownEnd = lastAttempt.StartTime.AddMinutes(exam.CooldownMinutes);
                    if (cooldownEnd > DateTime.UtcNow)
                    {
                        nextAttemptAvailableAt = cooldownEnd;
                    }
                }

                // Add exam with student-specific information
                result.Add(new ExamResponseDto
                {
                    ExamId = exam.ExamId,
                    Title = exam.Title,
                    MaxAttempts = exam.MaxAttempts,
                    CooldownMinutes = exam.CooldownMinutes,
                    LastModified = exam.LastModified,
                    RemainingAttempts = remainingAttempts,
                    NextAttemptAvailableAt = nextAttemptAvailableAt
                });
            }

            return result;
        }
    }
}