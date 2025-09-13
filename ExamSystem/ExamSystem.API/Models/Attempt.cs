namespace ExamSystem.API.Models
{
    /// <summary>
    /// Entity representing a student's attempt at an exam
    /// Tracks attempt progress, timing, and status information
    /// </summary>
    public class Attempt
    {
        /// <summary>
        /// Unique identifier for this attempt
        /// </summary>
        public Guid AttemptId { get; set; }

        /// <summary>
        /// Foreign key reference to the exam being attempted
        /// </summary>
        public Guid ExamId { get; set; }
        
        /// <summary>
        /// Navigation property to the associated exam
        /// Used by Entity Framework for relationship mapping
        /// </summary>
        public Exam Exam { get; set; } = null!;

        /// <summary>
        /// ID of the student making this attempt
        /// Uses JWT token ID, not necessarily a database user ID
        /// </summary>
        public Guid StudentId { get; set; }
        
        /// <summary>
        /// Navigation property to the student user (optional)
        /// May be null for JWT-based students without database records
        /// </summary>
        public User Student { get; set; } = null!;

        /// <summary>
        /// Sequential number of this attempt for the student on this exam
        /// Starts at 1 and increments for each new attempt
        /// </summary>
        public int AttemptNumber { get; set; }

        /// <summary>
        /// Current status of the attempt
        /// Valid values: "InProgress" (active attempt) or "Completed" (finished attempt)
        /// </summary>
        public string AttemptStatus { get; set; } = "InProgress";

        /// <summary>
        /// Timestamp when the attempt was started
        /// Used for cooldown calculations and attempt tracking
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp when the attempt was completed/submitted
        /// Null for in-progress attempts, set when attempt is submitted
        /// </summary>
        public DateTime? EndTime { get; set; }
    }
}