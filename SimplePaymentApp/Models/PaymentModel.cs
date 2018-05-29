using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SimplePaymentApp.Models
{
    public class PaymentModel
    {
        [Required]
        [StringLength(16)]
        [DefaultValue("4111111111111111")]
        [RegularExpression("[0-9]", ErrorMessage = "Number must be numeric")]
        public string Number { get; set; }

        [Required]
        [StringLength(4)]
        [RegularExpression("[0-9]", ErrorMessage = "CVV code must be numeric")]
        public string Code { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression("[^0-9]+", ErrorMessage = "Provide a valid name")]
        public string Name { get; set; }

        [Required]
        [Range(1, Double.MaxValue, ErrorMessage = "The Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

        [Required]
        [Display(Name = "Valid until")]
        [DataType(DataType.Date)]
        [FutureDate]
        public DateTime? Valid { get; set; }

        public bool? Result { get; set; }
    }
}
