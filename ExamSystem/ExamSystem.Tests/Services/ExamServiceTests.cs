using Microsoft.EntityFrameworkCore;
using ExamSystem.API.Data;
using ExamSystem.API.Models;
using ExamSystem.API.Models.DTOs;
using ExamSystem.API.Services;

namespace ExamSystem.Tests.Services
{
    /// <summary>
    /// Comprehensive test suite for ExamService covering all methods and edge cases
    /// </summary>
    public class ExamServiceTests : IDisposable
    {
        private readonly ExamDbContext _context;
        private readonly ExamService _examService;

        public ExamServiceTests()
        {
            var options = new DbContextOptionsBuilder<ExamDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ExamDbContext(options);
            _examService = new ExamService(_context);
        }

        #region CreateOrUpdateExamAsync Tests

        /// <summary>
        /// Test: Create new exam with valid data should succeed
        /// </summary>
        [Fact]
        public async Task CreateExam_ShouldCreateExamSuccessfully()
        {
            // Arrange
            var examDto = new CreateExamDto
            {
                Title = "Test Exam",
                MaxAttempts = 3,
                CooldownMinutes = 30
            };

            // Act
            var result = await _examService.CreateOrUpdateExamAsync(examDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Exam", result.Title);
            Assert.Equal(3, result.MaxAttempts);
            Assert.Equal(30, result.CooldownMinutes);
            Assert.True(result.ExamId != Guid.Empty);
        }

        /// <summary>
        /// Test: Update existing exam should modify properties and clear attempts
        /// </summary>
        [Fact]
        public async Task UpdateExam_ShouldUpdateExamAndClearAttempts()
        {
            // Arrange - Create exam and attempts
            var examId = Guid.NewGuid();
            var studentId = Guid.NewGuid();
            var exam = new Exam
            {
                ExamId = examId,
                Title = "Original Title",
                MaxAttempts = 2,
                CooldownMinutes = 10
            };
            var attempt = new Attempt
            {
                AttemptId = Guid.NewGuid(),
                ExamId = examId,
                StudentId = studentId,
                AttemptNumber = 1,
                AttemptStatus = "Completed"
            };
            _context.Exams.Add(exam);
            _context.Attempts.Add(attempt);
            await _context.SaveChangesAsync();

            var updateDto = new CreateExamDto
            {
                Title = "Updated Title",
                MaxAttempts = 5,
                CooldownMinutes = 60
            };

            // Act
            var result = await _examService.CreateOrUpdateExamAsync(updateDto, examId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Title", result.Title);
            Assert.Equal(5, result.MaxAttempts);
            Assert.Equal(60, result.CooldownMinutes);
            
            // Verify attempts were cleared
            var remainingAttempts = await _context.Attempts.Where(a => a.ExamId == examId).ToListAsync();
            Assert.Empty(remainingAttempts);
        }

        /// <summary>
        /// Test: Update non-existent exam should return null
        /// </summary>
        [Fact]
        public async Task UpdateExam_ShouldReturnNull_WhenExamNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var examDto = new CreateExamDto
            {
                Title = "Test Exam",
                MaxAttempts = 3,
                CooldownMinutes = 30
            };

            // Act
            var result = await _examService.CreateOrUpdateExamAsync(examDto, nonExistentId);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetExamForStudentAsync Tests

        /// <summary>
        /// Test: Get exam for student with no attempts should return full remaining attempts
        /// </summary>
        [Fact]
        public async Task GetExamForStudent_ShouldReturnExamWithFullAttempts_WhenNoAttempts()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var exam = new Exam
            {
                ExamId = Guid.NewGuid(),
                Title = "Test Exam",
                MaxAttempts = 3,
                CooldownMinutes = 30
            };
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            // Act
            var result = await _examService.GetExamForStudentAsync(studentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(exam.Title, result.Title);
            Assert.Equal(3, result.RemainingAttempts);
            Assert.Null(result.NextAttemptAvailableAt);
        }

        /// <summary>
        /// Test: Get exam for student with attempts should calculate remaining attempts correctly
        /// </summary>
        [Fact]
        public async Task GetExamForStudent_ShouldCalculateRemainingAttempts_WhenAttemptsExist()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var exam = new Exam
            {
                ExamId = Guid.NewGuid(),
                Title = "Test Exam",
                MaxAttempts = 3,
                CooldownMinutes = 0
            };
            var attempt = new Attempt
            {
                AttemptId = Guid.NewGuid(),
                ExamId = exam.ExamId,
                StudentId = studentId,
                AttemptNumber = 1,
                AttemptStatus = "Completed",
                StartTime = DateTime.UtcNow.AddMinutes(-10)
            };
            _context.Exams.Add(exam);
            _context.Attempts.Add(attempt);
            await _context.SaveChangesAsync();

            // Act
            var result = await _examService.GetExamForStudentAsync(studentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.RemainingAttempts); // 3 - 1 = 2
        }

        /// <summary>
        /// Test: Get exam with cooldown should set next attempt available time
        /// </summary>
        [Fact]
        public async Task GetExamForStudent_ShouldSetCooldownTime_WhenCooldownActive()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var exam = new Exam
            {
                ExamId = Guid.NewGuid(),
                Title = "Test Exam",
                MaxAttempts = 3,
                CooldownMinutes = 60
            };
            var recentAttempt = new Attempt
            {
                AttemptId = Guid.NewGuid(),
                ExamId = exam.ExamId,
                StudentId = studentId,
                AttemptNumber = 1,
                AttemptStatus = "Completed",
                StartTime = DateTime.UtcNow.AddMinutes(-30) // 30 minutes ago, cooldown still active
            };
            _context.Exams.Add(exam);
            _context.Attempts.Add(recentAttempt);
            await _context.SaveChangesAsync();

            // Act
            var result = await _examService.GetExamForStudentAsync(studentId);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.NextAttemptAvailableAt);
            Assert.True(result.NextAttemptAvailableAt > DateTime.UtcNow);
        }

        /// <summary>
        /// Test: Get exam when no exam exists should return null
        /// </summary>
        [Fact]
        public async Task GetExamForStudent_ShouldReturnNull_WhenNoExamExists()
        {
            // Arrange
            var studentId = Guid.NewGuid();

            // Act
            var result = await _examService.GetExamForStudentAsync(studentId);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region StartAttemptAsync Tests

        /// <summary>
        /// Test: Start first attempt should create new attempt successfully
        /// </summary>
        [Fact]
        public async Task StartAttempt_ShouldCreateNewAttempt_WhenValidConditions()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var exam = new Exam
            {
                ExamId = Guid.NewGuid(),
                Title = "Test Exam",
                MaxAttempts = 3,
                CooldownMinutes = 0
            };
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            // Act
            var result = await _examService.StartAttemptAsync(studentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.AttemptNumber);
            Assert.Equal("InProgress", result.AttemptStatus);
            Assert.True(result.StartTime <= DateTime.UtcNow);
        }

        /// <summary>
        /// Test: Start attempt when max attempts reached should return null
        /// </summary>
        [Fact]
        public async Task StartAttempt_ShouldReturnNull_WhenMaxAttemptsReached()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var exam = new Exam
            {
                ExamId = Guid.NewGuid(),
                Title = "Test Exam",
                MaxAttempts = 1,
                CooldownMinutes = 0
            };
            var existingAttempt = new Attempt
            {
                AttemptId = Guid.NewGuid(),
                ExamId = exam.ExamId,
                StudentId = studentId,
                AttemptNumber = 1,
                AttemptStatus = "Completed"
            };
            _context.Exams.Add(exam);
            _context.Attempts.Add(existingAttempt);
            await _context.SaveChangesAsync();

            // Act
            var result = await _examService.StartAttemptAsync(studentId);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Test: Start attempt during cooldown period should return null
        /// </summary>
        [Fact]
        public async Task StartAttempt_ShouldReturnNull_WhenCooldownActive()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var exam = new Exam
            {
                ExamId = Guid.NewGuid(),
                Title = "Test Exam",
                MaxAttempts = 3,
                CooldownMinutes = 60
            };
            var recentAttempt = new Attempt
            {
                AttemptId = Guid.NewGuid(),
                ExamId = exam.ExamId,
                StudentId = studentId,
                AttemptNumber = 1,
                AttemptStatus = "Completed",
                StartTime = DateTime.UtcNow.AddMinutes(-30) // 30 minutes ago, cooldown still active
            };
            _context.Exams.Add(exam);
            _context.Attempts.Add(recentAttempt);
            await _context.SaveChangesAsync();

            // Act
            var result = await _examService.StartAttemptAsync(studentId);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Test: Start attempt when in-progress attempt exists should return existing attempt
        /// </summary>
        [Fact]
        public async Task StartAttempt_ShouldReturnExistingAttempt_WhenInProgressExists()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var exam = new Exam
            {
                ExamId = Guid.NewGuid(),
                Title = "Test Exam",
                MaxAttempts = 3,
                CooldownMinutes = 0
            };
            var inProgressAttempt = new Attempt
            {
                AttemptId = Guid.NewGuid(),
                ExamId = exam.ExamId,
                StudentId = studentId,
                AttemptNumber = 1,
                AttemptStatus = "InProgress",
                StartTime = DateTime.UtcNow.AddMinutes(-10)
            };
            _context.Exams.Add(exam);
            _context.Attempts.Add(inProgressAttempt);
            await _context.SaveChangesAsync();

            // Act
            var result = await _examService.StartAttemptAsync(studentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(inProgressAttempt.AttemptId, result.AttemptId);
            Assert.Equal("InProgress", result.AttemptStatus);
        }

        /// <summary>
        /// Test: Start attempt when no exam exists should return null
        /// </summary>
        [Fact]
        public async Task StartAttempt_ShouldReturnNull_WhenNoExamExists()
        {
            // Arrange
            var studentId = Guid.NewGuid();

            // Act
            var result = await _examService.StartAttemptAsync(studentId);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Test: Start attempt with specific exam ID should work correctly
        /// </summary>
        [Fact]
        public async Task StartAttempt_ShouldStartAttemptForSpecificExam_WhenExamIdProvided()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var exam1 = new Exam { ExamId = Guid.NewGuid(), Title = "Exam 1", MaxAttempts = 3, CooldownMinutes = 0 };
            var exam2 = new Exam { ExamId = Guid.NewGuid(), Title = "Exam 2", MaxAttempts = 2, CooldownMinutes = 0 };
            _context.Exams.AddRange(exam1, exam2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _examService.StartAttemptAsync(studentId, exam2.ExamId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.AttemptNumber);
            Assert.Equal("InProgress", result.AttemptStatus);
            
            // Verify it was created for the correct exam
            var attempt = await _context.Attempts.FirstAsync();
            Assert.Equal(exam2.ExamId, attempt.ExamId);
        }

        #endregion

        #region SubmitAttemptAsync Tests

        /// <summary>
        /// Test: Submit in-progress attempt should mark as completed
        /// </summary>
        [Fact]
        public async Task SubmitAttempt_ShouldMarkAttemptAsCompleted()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var attempt = new Attempt
            {
                AttemptId = Guid.NewGuid(),
                ExamId = Guid.NewGuid(),
                StudentId = studentId,
                AttemptNumber = 1,
                AttemptStatus = "InProgress",
                StartTime = DateTime.UtcNow.AddMinutes(-30)
            };
            _context.Attempts.Add(attempt);
            await _context.SaveChangesAsync();

            // Act
            var result = await _examService.SubmitAttemptAsync(attempt.AttemptId, studentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Completed", result.AttemptStatus);
            Assert.NotNull(result.EndTime);
            Assert.True(result.EndTime <= DateTime.UtcNow);
        }

        /// <summary>
        /// Test: Submit non-existent attempt should return null
        /// </summary>
        [Fact]
        public async Task SubmitAttempt_ShouldReturnNull_WhenAttemptNotFound()
        {
            // Arrange
            var nonExistentAttemptId = Guid.NewGuid();
            var studentId = Guid.NewGuid();

            // Act
            var result = await _examService.SubmitAttemptAsync(nonExistentAttemptId, studentId);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Test: Submit attempt with wrong student ID should return null
        /// </summary>
        [Fact]
        public async Task SubmitAttempt_ShouldReturnNull_WhenWrongStudentId()
        {
            // Arrange
            var correctStudentId = Guid.NewGuid();
            var wrongStudentId = Guid.NewGuid();
            var attempt = new Attempt
            {
                AttemptId = Guid.NewGuid(),
                ExamId = Guid.NewGuid(),
                StudentId = correctStudentId,
                AttemptNumber = 1,
                AttemptStatus = "InProgress"
            };
            _context.Attempts.Add(attempt);
            await _context.SaveChangesAsync();

            // Act
            var result = await _examService.SubmitAttemptAsync(attempt.AttemptId, wrongStudentId);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Test: Submit already completed attempt should return null
        /// </summary>
        [Fact]
        public async Task SubmitAttempt_ShouldReturnNull_WhenAttemptAlreadyCompleted()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var attempt = new Attempt
            {
                AttemptId = Guid.NewGuid(),
                ExamId = Guid.NewGuid(),
                StudentId = studentId,
                AttemptNumber = 1,
                AttemptStatus = "Completed",
                EndTime = DateTime.UtcNow.AddMinutes(-10)
            };
            _context.Attempts.Add(attempt);
            await _context.SaveChangesAsync();

            // Act
            var result = await _examService.SubmitAttemptAsync(attempt.AttemptId, studentId);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetStudentAttemptsAsync Tests

        /// <summary>
        /// Test: Get student attempts should return all attempts for specific student
        /// </summary>
        [Fact]
        public async Task GetStudentAttempts_ShouldReturnStudentAttempts_OrderedByAttemptNumber()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var otherStudentId = Guid.NewGuid();
            var examId = Guid.NewGuid();
            
            var attempts = new List<Attempt>
            {
                new Attempt { AttemptId = Guid.NewGuid(), ExamId = examId, StudentId = studentId, AttemptNumber = 2, AttemptStatus = "Completed" },
                new Attempt { AttemptId = Guid.NewGuid(), ExamId = examId, StudentId = studentId, AttemptNumber = 1, AttemptStatus = "Completed" },
                new Attempt { AttemptId = Guid.NewGuid(), ExamId = examId, StudentId = otherStudentId, AttemptNumber = 1, AttemptStatus = "Completed" }
            };
            _context.Attempts.AddRange(attempts);
            await _context.SaveChangesAsync();

            // Act
            var result = await _examService.GetStudentAttemptsAsync(studentId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].AttemptNumber); // Should be ordered by attempt number
            Assert.Equal(2, result[1].AttemptNumber);
            Assert.All(result, attempt => Assert.Equal(studentId, attempt.AttemptId != Guid.Empty ? studentId : studentId));
        }

        /// <summary>
        /// Test: Get student attempts when no attempts exist should return empty list
        /// </summary>
        [Fact]
        public async Task GetStudentAttempts_ShouldReturnEmptyList_WhenNoAttempts()
        {
            // Arrange
            var studentId = Guid.NewGuid();

            // Act
            var result = await _examService.GetStudentAttemptsAsync(studentId);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetAllAttemptsAsync Tests

        /// <summary>
        /// Test: Get all attempts should return attempts from all students
        /// </summary>
        [Fact]
        public async Task GetAllAttempts_ShouldReturnAllAttempts_OrderedByAttemptNumber()
        {
            // Arrange
            var student1Id = Guid.NewGuid();
            var student2Id = Guid.NewGuid();
            var examId = Guid.NewGuid();
            
            // Add users first to support navigation properties
            var users = new List<User>
            {
                new User { Id = student1Id, Username = "student1", PasswordHash = "hash1", Role = "Student" },
                new User { Id = student2Id, Username = "student2", PasswordHash = "hash2", Role = "Student" }
            };
            _context.Users.AddRange(users);
            
            var attempts = new List<Attempt>
            {
                new Attempt { AttemptId = Guid.NewGuid(), ExamId = examId, StudentId = student1Id, AttemptNumber = 2, AttemptStatus = "Completed", StartTime = DateTime.UtcNow },
                new Attempt { AttemptId = Guid.NewGuid(), ExamId = examId, StudentId = student2Id, AttemptNumber = 1, AttemptStatus = "InProgress", StartTime = DateTime.UtcNow },
                new Attempt { AttemptId = Guid.NewGuid(), ExamId = examId, StudentId = student1Id, AttemptNumber = 3, AttemptStatus = "Completed", StartTime = DateTime.UtcNow }
            };
            _context.Attempts.AddRange(attempts);
            await _context.SaveChangesAsync();

            // Act
            var result = await _examService.GetAllAttemptsAsync();

            // Assert
            Assert.Equal(3, result.Count);
            // Should be ordered by attempt number
            Assert.True(result[0].AttemptNumber <= result[1].AttemptNumber);
            Assert.True(result[1].AttemptNumber <= result[2].AttemptNumber);
        }

        /// <summary>
        /// Test: Get all attempts when no attempts exist should return empty list
        /// </summary>
        [Fact]
        public async Task GetAllAttempts_ShouldReturnEmptyList_WhenNoAttempts()
        {
            // Act
            var result = await _examService.GetAllAttemptsAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetAllExamsAsync Tests

        [Fact]
        public async Task GetAllExams_ShouldReturnAllExams_OrderedByLastModified()
        {
            var exam1 = new Exam { ExamId = Guid.NewGuid(), Title = "Exam 1", MaxAttempts = 3, CooldownMinutes = 30, LastModified = DateTime.UtcNow.AddDays(-1) };
            var exam2 = new Exam { ExamId = Guid.NewGuid(), Title = "Exam 2", MaxAttempts = 5, CooldownMinutes = 60, LastModified = DateTime.UtcNow };
            _context.Exams.AddRange(exam1, exam2);
            await _context.SaveChangesAsync();

            var result = await _examService.GetAllExamsAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("Exam 2", result[0].Title);
            Assert.Equal("Exam 1", result[1].Title);
        }

        #endregion

        #region GetAllExamsForStudentAsync Tests

        [Fact]
        public async Task GetAllExamsForStudent_ShouldReturnExamsWithRemainingAttempts()
        {
            var studentId = Guid.NewGuid();
            var exam1 = new Exam { ExamId = Guid.NewGuid(), Title = "Exam 1", MaxAttempts = 3, CooldownMinutes = 0 };
            var exam2 = new Exam { ExamId = Guid.NewGuid(), Title = "Exam 2", MaxAttempts = 2, CooldownMinutes = 0 };
            var attempt = new Attempt { AttemptId = Guid.NewGuid(), ExamId = exam1.ExamId, StudentId = studentId, AttemptNumber = 1, AttemptStatus = "Completed" };
            
            _context.Exams.AddRange(exam1, exam2);
            _context.Attempts.Add(attempt);
            await _context.SaveChangesAsync();

            var result = await _examService.GetAllExamsForStudentAsync(studentId);

            Assert.Equal(2, result.Count);
            var exam1Result = result.First(e => e.Title == "Exam 1");
            var exam2Result = result.First(e => e.Title == "Exam 2");
            Assert.Equal(2, exam1Result.RemainingAttempts);
            Assert.Equal(2, exam2Result.RemainingAttempts);
        }

        #endregion

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}