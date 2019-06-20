using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.Areas.UserManagement.Models
{
    public class UserPasswordResetViewModel
    {
        public string Id { get; set; }
        [Editable(false)]
        public string UserName { get; set; }
        [Editable(false)]
        [EmailAddress]
        public string Email { get; set; }
        [Editable(false)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Editable(false)]
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }
        [Editable(false)]
        public string Surname { get; set; }
        [Editable(false)]
        public string EmployeeNumber { get; set; }
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name = "Confirm Password")]
        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
