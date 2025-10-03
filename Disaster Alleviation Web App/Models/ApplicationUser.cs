using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Disaster_Alleviation_Web_App.Models
{
    public class ApplicationUser : IdentityUser
    {
        //public string FirstName { get; set; }
        //public string LastName { get; set; }
        public string FullName { get; set; }
        
        public string Skills { get; set; }
        public string Availability {  get; set; }
        public string CellphoneNumber { get; set; }
        public string Address { get; set; }
        //public string City { get; set; }
        //public string PostalCode { get; set; }
      //  public DateTime? CreatedDate { get; set; }
      //  public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual ICollection<Donation> Donations { get; set; }
    
    }
}