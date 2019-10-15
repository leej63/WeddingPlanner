using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace WeddingPlanner.Models
{
    public class Wedding
    {
        [Key]
        [Required]
        public int WeddingId {get;set;}

        [Required(ErrorMessage = "Must provide a name.")]
        [Display(Name = "Wedder One: ")]
        public string WedderOne {get;set;}

        [Required(ErrorMessage = "Must provide a name.")]
        [Display(Name = "Wedder Two: ")]
        public string WedderTwo {get;set;}

        [Required(ErrorMessage = "Must provide a wedding date")]
        [Display(Name = "Date: ")]
        [FutureDate]
        public DateTime Date {get;set;}

        [Required(ErrorMessage = "Must provide a location address.")]
        [Display(Name = "Wedding Address: ")]
        public string Address {get;set;}

        public int UserId {get;set;}
        public List<Attendee> Atendees {get;set;}

        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;
    }

    public class FutureDate : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            DateTime today = DateTime.Now;
            if(today > (DateTime)value)
            {
                return new ValidationResult("Wedding must be in the future.");
            }
            return ValidationResult.Success;
        }
    }
}