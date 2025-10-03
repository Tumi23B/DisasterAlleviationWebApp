using System.Collections.Generic;

namespace Disaster_Alleviation_Web_App.Models
{
    public class UserWithRoleViewModel
    {
        public ApplicationUser User { get; set; }
        public IList<string> Roles { get; set; } 
    }
}
