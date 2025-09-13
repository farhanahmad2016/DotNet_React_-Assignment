using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ExamSystem.API.Controllers;
using ExamSystem.API.Models.DTOs;
using ExamSystem.API.Services;
using ExamSystem.API.Validators;
using ExamSystem.API.Models;

namespace ExamSystem.Tests.Controllers
{
    /// <summary>
    /// Comprehensive test suite for ExamController focusing on service interactions
    /// </summary>
    public class ExamControllerTests
    {
        private readonly Mock<IExamService> _mockExamService;
        private readonly CreateExamDtoValidator _validator;
        private readonly ExamController _controller;

        public ExamControllerTests()
        {
            _mockExamService = new Mock<IExamService>();
            _validator = new CreateExamDtoValidator();
            _controller = new ExamController(_mockExamService.Object, _validator);
        }

        #region CreateExam Tests

        /// <summary>
        /// Test: Create exam should return BadRequest when service returns null
        /// </summary>
        [Fact]
        public async Task CreateExam_ShouldReturnBadRequest_WhenServiceFails()
        {
            // Arrange
            SetupAdminControllerContext(Guid.NewGuid());
            var examDto = new CreateExamDto
            {
                Title = "Test Exam",
                MaxAttempts = 3,
                CooldownMinutes = 30
            };

            _mockExamService.Setup(s => s.CreateOrUpdateExamAsync(examDto, null))
                .ReturnsAsync((Exam?)null);

            // Act
            var result = await _controller.CreateExam(examDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to create exam", badRequestResult.Value);
        }

        /// <summary>
        /// Test: Create exam should return 500 when exception occurs
        /// </summary>
        [Fact]
        public async Task CreateExam_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            SetupAdminControllerContext(Guid.NewGuid());
            var examDto = new CreateExamDto
            {
                Title = "Test Exam",
                MaxAttempts = 3,
                CooldownMinutes = 30
            };

            _mockExamService.Setup(s => s.CreateOrUpdateExamAsync(examDto, null))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateExam(examDto);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        #endregion

        #region UpdateExam Tests

        /// <summary>
        /// Test: Update exam should return NotFound when exam doesn't exist
        /// </summary>
        [Fact]
        public async Task UpdateExam_ShouldReturnNotFound_WhenExamNotExists()
        {
            // Arrange
            SetupAdminControllerContext(Guid.NewGuid());
            var examId = Guid.NewGuid();
            var examDto = new CreateExamDto
            {
                Title = "Updated Exam",
                MaxAttempts = 5,
                CooldownMinutes = 60
            };

            _mockExamService.Setup(s => s.CreateOrUpdateExamAsync(examDto, examId))
                .ReturnsAsync((Exam?)null);

            // Act
            var result = await _controller.UpdateExam(examId, examDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Exam not found", notFoundResult.Value);
        }

        /// <summary>
        /// Test: Update exam should return 500 when exception occurs
        /// </summary>
        [Fact]
        public async Task UpdateExam_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            SetupAdminControllerContext(Guid.NewGuid());
            var examId = Guid.NewGuid();
            var examDto = new CreateExamDto
            {
                Title = "Updated Exam",
                MaxAttempts = 5,
                CooldownMinutes = 60
            };

            _mockExamService.Setup(s => s.CreateOrUpdateExamAsync(examDto, examId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.UpdateExam(examId, examDto);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        #endregion

        #region GetExamForStudent Tests

        /// <summary>
        /// Test: Get exam for student should return Ok when exam exists
        /// </summary>
        [Fact]
        public async Task GetExamForStudent_ShouldReturnOk_WhenExamExists()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            SetupControllerContext(studentId);

            var examResponse = new ExamResponseDto
            {
                ExamId = Guid.NewGuid(),
                Title = "Test Exam",
                MaxAttempts = 3,
                CooldownMinutes = 30,
                RemainingAttempts = 3,
                NextAttemptAvailableAt = null
            };

            _mockExamService.Setup(s => s.GetExamForStudentAsync(studentId))
                .ReturnsAsync(examResponse);

            // Act
            var result = await _controller.GetExamForStudent();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedExam = Assert.IsType<ExamResponseDto>(okResult.Value);
            Assert.Equal("Test Exam", returnedExam.Title);
            Assert.Equal(3, returnedExam.RemainingAttempts);
        }

        /// <summary>
        /// Test: Get exam for student should return NotFound when no exam exists
        /// </summary>
        [Fact]
        public async Task GetExamForStudent_ShouldReturnNotFound_WhenNoExamExists()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            SetupControllerContext(studentId);

            _mockExamService.Setup(s => s.GetExamForStudentAsync(studentId))
                .ReturnsAsync((ExamResponseDto?)null);

            // Act
            var result = await _controller.GetExamForStudent();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No exam found", notFoundResult.Value);
        }

        /// <summary>
        /// Test: Get exam for student should return 500 when exception occurs
        /// </summary>
        [Fact]
        public async Task GetExamForStudent_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            SetupControllerContext(studentId);

            _mockExamService.Setup(s => s.GetExamForStudentAsync(studentId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetExamForStudent();

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        /// <summary>
        /// Test: Get exam for student with cooldown should include next attempt time
        /// </summary>
        [Fact]
        public async Task GetExamForStudent_ShouldIncludeCooldownInfo_WhenCooldownActive()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            SetupControllerContext(studentId);

            var nextAttemptTime = DateTime.UtcNow.AddMinutes(30);
            var examResponse = new ExamResponseDto
            {
                ExamId = Guid.NewGuid(),
                Title = "Test Exam",
                MaxAttempts = 3,
                CooldownMinutes = 60,
                RemainingAttempts = 2,
                NextAttemptAvailableAt = nextAttemptTime
            };

            _mockExamService.Setup(s => s.GetExamForStudentAsync(studentId))
                .ReturnsAsync(examResponse);

            // Act
            var result = await _controller.GetExamForStudent();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedExam = Assert.IsType<ExamResponseDto>(okResult.Value);
            Assert.Equal(2, returnedExam.RemainingAttempts);
            Assert.NotNull(returnedExam.NextAttemptAvailableAt);
            Assert.Equal(nextAttemptTime, returnedExam.NextAttemptAvailableAt);
        }

        /// <summary>
        /// Test: Get exam for student with no remaining attempts
        /// </summary>
        [Fact]
        public async Task GetExamForStudent_ShouldShowZeroAttempts_WhenMaxAttemptsReached()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            SetupControllerContext(studentId);

            var examResponse = new ExamResponseDto
            {
                ExamId = Guid.NewGuid(),
                Title = "Test Exam",
                MaxAttempts = 3,
                CooldownMinutes = 30,
                RemainingAttempts = 0, // No attempts left
                NextAttemptAvailableAt = null
            };

            _mockExamService.Setup(s => s.GetExamForStudentAsync(studentId))
                .ReturnsAsync(examResponse);

            // Act
            var result = await _controller.GetExamForStudent();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedExam = Assert.IsType<ExamResponseDto>(okResult.Value);
            Assert.Equal(0, returnedExam.RemainingAttempts);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper method to setup controller context with authenticated user
        /// </summary>
        /// <param name="studentId">Student ID to include in claims</param>
        private void SetupControllerContext(Guid studentId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, studentId.ToString()),
                new Claim(ClaimTypes.Role, "Student")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        /// <summary>
        /// Helper method to setup controller context with admin user
        /// </summary>
        /// <param name="adminId">Admin ID to include in claims</param>
        private void SetupAdminControllerContext(Guid adminId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, adminId.ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        #endregion
    }
}