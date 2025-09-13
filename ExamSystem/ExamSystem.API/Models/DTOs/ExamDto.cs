namespace ExamSystem.API.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object for creating or updating exams
    /// Contains the essential exam configuration data required from clients
    /// </summary>
    public class CreateExamDto
    {
        /// <summary>
        /// Display name/title of the exam
        /// Must be between 5-200 characters and contain only allowed characters
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// Maximum number of attempts allowed per student for this exam
        /// Must be between 1 and 1000
        /// </summary>
        public int MaxAttempts { get; set; }
        
        /// <summary>
        /// Cooldown period in minutes between attempts
        /// Must be between 0 and 525600 minutes (1 year)
        /// Only applicable when MaxAttempts > 1
        /// </summary>
        public int CooldownMinutes { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for exam responses to clients
    /// Includes exam configuration plus student-specific attempt information
    /// </summary>
    public class ExamResponseDto
    {
        /// <summary>
        /// Unique identifier for the exam
        /// </summary>
        public Guid ExamId { get; set; }
        
        /// <summary>
        /// Display name/title of the exam
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// Maximum number of attempts allowed per student
        /// </summary>
        public int MaxAttempts { get; set; }
        
        /// <summary>
        /// Cooldown period in minutes between attempts
        /// </summary>
        public int CooldownMinutes { get; set; }
        
        /// <summary>
        /// Timestamp of when the exam was last modified
        /// </summary>
        public DateTime LastModified { get; set; }
        
        /// <summary>
        /// Number of attempts remaining for the requesting student
        /// Set to 0 for admin views where student context is not applicable
        /// </summary>
        public int RemainingAttempts { get; set; }
        
        /// <summary>
        /// Timestamp when the student can make their next attempt
        /// Null if no cooldown is active or not applicable
        /// </summary>
        public DateTime? NextAttemptAvailableAt { get; set; }
    }
}
