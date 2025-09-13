using FluentValidation;
using ExamSystem.API.Models.DTOs;

namespace ExamSystem.API.Validators
{
    /// <summary>
    /// FluentValidation validator for exam creation and update operations.
    /// Provides comprehensive validation including business rules and data integrity checks.
    /// </summary>
    public class CreateExamDtoValidator : AbstractValidator<CreateExamDto>
    {
        /// <summary>
        /// Initializes validation rules for CreateExamDto
        /// </summary>
        public CreateExamDtoValidator()
        {
            // Title validation: Required, length constraints, and character format
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Exam title is required")
                .Length(5, 200).WithMessage("Title must be between 5 and 200 characters")
                .Matches(@"^[a-zA-Z0-9\s\-_.,!?()]+$")
                .WithMessage("Title can only contain letters, numbers, spaces, and basic punctuation (- _ . , ! ? ( ))");

            // Max attempts validation: Must be within reasonable bounds
            RuleFor(x => x.MaxAttempts)
                .InclusiveBetween(1, 1000)
                .WithMessage("Max attempts must be between 1 and 1000");

            // Cooldown validation: Range check (0 to 1 year in minutes)
            RuleFor(x => x.CooldownMinutes)
                .InclusiveBetween(0, 525600)
                .WithMessage("Cooldown must be between 0 and 525600 minutes (1 year)");

            // Business rule validation: Cooldown logic only applies to multiple attempts
            // This prevents illogical configurations where cooldown is set but only 1 attempt is allowed
            RuleFor(x => x.CooldownMinutes)
                .Must((dto, cooldown) => cooldown == 0 || dto.MaxAttempts > 1)
                .WithMessage("Cooldown period is only applicable when max attempts is greater than 1")
                .When(x => x.CooldownMinutes > 0);
        }
    }
}