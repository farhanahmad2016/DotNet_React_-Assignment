using Microsoft.AspNetCore.Mvc;
using ExamSystem.API.Models.DTOs;
using ExamSystem.API.Services;
using ExamSystem.API.Validators;
using ExamSystem.API.Extensions;


namespace ExamSystem.API.Controllers
{
    /// <summary>
    /// Handles authentication operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly LoginDtoValidator _loginValidator;

        /// <summary>
        /// Initializes a new instance of AuthController
        /// </summary>
        /// <param name="authService">Authentication service</param>
        /// <param name="loginValidator">Login validation service</param>
        public AuthController(IAuthService authService, LoginDtoValidator loginValidator)
        {
            _authService = authService;
            _loginValidator = loginValidator;
        }

        /// <summary>
        /// Authenticates user and returns JWT token
        /// </summary>
        /// <param name="loginDto">Login credentials (validated by FluentValidation)</param>
        /// <returns>JWT token if successful, error message if failed</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // Use FluentValidation for enhanced validation
            var validationResult = await this.ValidateAsync(loginDto, _loginValidator);
            if (validationResult != null) return validationResult;

            try
            {
                var token = await _authService.LoginAsync(loginDto);
                if (token == null)
                    return Unauthorized("Invalid credentials");

                return Ok(new { token });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred during login");
            }
        }
    }
}