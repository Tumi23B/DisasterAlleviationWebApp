
using Disaster_Alleviation_Web_App.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Disaster_Alleviation_Web_App.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public IEnumerable<object> HelperTasks { get; internal set; }

        // DbSets for your other entities later
        // public DbSet<Incident> Incidents { get; set; }
        // public DbSet<Donation> Donations { get; set; }
        // public DbSet<VolunteerTask> VolunteerTasks { get; set; }
    }
}
