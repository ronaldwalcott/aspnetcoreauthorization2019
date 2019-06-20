using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.Areas.UserManagement.Constants
{
    public static class ClaimNames
    {
        public static List<string> ClaimName = new List<string>() {
            PolicyNames.AccessToTest1ScreenPolicy,
            PolicyNames.AccessToTest2ScreenPolicy
        };
    }

}
