using System.ComponentModel.DataAnnotations;

namespace Task_Manager_Care.Validation
{
    public class NotInFutureAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime date && date.Date > DateTime.UtcNow.Date)
            {
                return new ValidationResult(ErrorMessage ?? "The date cannot be in the future.");
            }
            return ValidationResult.Success;
        }
    }
}
