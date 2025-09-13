using FluentValidation;
using ExamSystem.API.Models.DTOs;

namespace ExamSystem.API.Validators
{
    /// <summary>
    /// FluentValidation validator for user login credentials.
    /// Enforces security standards and format requirements for authentication.
    /// </summary>
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        /// <summary>
        /// Initializes validation rules for login credentials
        /// </summary>
        public LoginDtoValidator()
        {
            // Username validation: Required, length constraints, and secure character format
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .Length(3, 50).WithMessage("Username must be between 3 and 50 characters")
                .Matches(@"^[a-zA-Z0-9_]+$")
                .WithMessage("Username can only contain letters, numbers, and underscores");

            // Password validation: Required with minimum security length
            // Note: Additional password complexity rules can be added here for production
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long");
        }
    }
}