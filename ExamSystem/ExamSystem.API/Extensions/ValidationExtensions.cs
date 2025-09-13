using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ExamSystem.API.Extensions
{
    /// <summary>
    /// Extension methods for integrating FluentValidation with ASP.NET Core controllers
    /// Provides manual validation capabilities with proper error handling and ModelState integration
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>
        /// Validates a model using FluentValidation and returns appropriate HTTP response
        /// Integrates validation errors with ASP.NET Core ModelState for consistent error formatting
        /// </summary>
        /// <typeparam name="T">Type of the model being validated</typeparam>
        /// <param name="controller">Controller instance for accessing ModelState</param>
        /// <param name="model">Model instance to validate</param>
        /// <param name="validator">FluentValidation validator for the model type</param>
        /// <returns>
        /// BadRequest with ModelState errors if validation fails,
        /// null if validation passes (allowing controller to continue processing)
        /// </returns>
        public static async Task<IActionResult?> ValidateAsync<T>(this ControllerBase controller, T model, IValidator<T> validator)
        {
            // Execute FluentValidation rules asynchronously
            var validationResult = await validator.ValidateAsync(model);
            
            // If validation failed, add errors to ModelState and return BadRequest
            if (!validationResult.IsValid)
            {
                // Add each validation error to ModelState for consistent API error responses
                foreach (var error in validationResult.Errors)
                {
                    controller.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return controller.BadRequest(controller.ModelState);
            }
            
            // Return null to indicate validation passed - controller can continue processing
            return null;
        }
    }
}