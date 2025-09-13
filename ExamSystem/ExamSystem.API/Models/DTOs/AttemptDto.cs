namespace ExamSystem.API.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object for exam attempt responses
    /// Used to return attempt information to clients without exposing internal entity structure
    /// </summary>
    public class AttemptResponseDto
    {
        /// <summary>
        /// Unique identifier for the attempt
        /// </summary>
        public Guid AttemptId { get; set; }
        
        /// <summary>
        /// Sequential number of this attempt for the student on the specific exam
        /// Starts at 1 and increments for each new attempt
        /// </summary>
        public int AttemptNumber { get; set; }
        
        /// <summary>
        /// Current status of the attempt
        /// Values: "InProgress" (active) or "Completed" (finished)
        /// </summary>
        public string AttemptStatus { get; set; } = string.Empty;
        
        /// <summary>
        /// Timestamp when the attempt was started
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// Timestamp when the attempt was completed/submitted
        /// Null for in-progress attempts
        /// </summary>
        public DateTime? EndTime { get; set; }
        
        /// <summary>
        /// Title of the exam being attempted
        /// Included for better user experience in attempt history
        /// </summary>
        public string ExamTitle { get; set; } = string.Empty;
    }
}
