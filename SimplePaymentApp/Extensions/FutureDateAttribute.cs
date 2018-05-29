using System.ComponentModel.DataAnnotations;
using System;

namespace SimplePaymentApp
{
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(
            object value, ValidationContext validationContext)
        {
            var now = DateTime.UtcNow;
            var date = value as DateTime?;
            if (date.HasValue && (now.Year < date.Value.Year || 
                                  now.Year == date.Value.Year && now.Month <= date.Value.Month))
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("Date must be a future date");
            }
        }
    }

}