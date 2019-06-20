using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.Areas.UserManagement.Models
{
    public class UserLockoutViewModel
    {
        public string Id { get; set; }
        [Editable(false)]
        public string UserName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }
        public string Surname { get; set; }
        public string EmployeeNumber { get; set; }

    }
}
