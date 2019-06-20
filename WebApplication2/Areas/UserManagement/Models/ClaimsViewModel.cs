using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.Areas.UserManagement.Models
{
    public class ClaimsViewModel
    {
        public string Id { get; set; }
        public string ClaimName { get; set; }
        public Boolean HasClaim { get; set; }
    }
}
