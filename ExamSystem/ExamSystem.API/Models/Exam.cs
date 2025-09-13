using System.ComponentModel.DataAnnotations;

namespace ExamSystem.API.Models
{
    /// <summary>
    /// Entity representing an exam in the system
    /// Contains exam configuration and tracks associated attempts
    /// </summary>
    public class Exam
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
        /// Maximum number of attempts allowed per student for this exam
        /// </summary>
        public int MaxAttempts { get; set; }
        
        /// <summary>
        /// Cooldown period in minutes between attempts
        /// Students must wait this long after an attempt before starting another
        /// </summary>
        public int CooldownMinutes { get; set; }
        
        /// <summary>
        /// Timestamp of when the exam was last modified
        /// Used for ordering and tracking changes
        /// </summary>
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Navigation property for all attempts associated with this exam
        /// Used by Entity Framework for relationship mapping
        /// </summary>
        public List<Attempt> Attempts { get; set; } = new();
    }
}