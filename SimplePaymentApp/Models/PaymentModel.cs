using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SimplePaymentApp.Models
{
    public class PaymentModel : BaseViewModel
    {
        [Required]
        [StringLength(16)]
        [DefaultValue("4111111111111111")]
        [RegularExpression("[0-9]+", ErrorMessage = "Must be numeric")]
        public string Number { get; set; }

        [Required]
        [StringLength(4)]
        [RegularExpression("[0-9]+", ErrorMessage = "Must be numeric")]
        public string Code { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression("[^0-9]{4,}", ErrorMessage = "Provide a valid name")]
        public string Name { get; set; }

        [Required]
        [DefaultValue(1.0)]
        [Range(1, Double.MaxValue, ErrorMessage = "Must be greater than 0")]
        public double Quantity { get; set; }

        [Required]
        [Display(Name = "Valid until")]
        [DataType(DataType.Date)]
        [FutureDate]
        public DateTime? Valid { get; set; }
    }
}
