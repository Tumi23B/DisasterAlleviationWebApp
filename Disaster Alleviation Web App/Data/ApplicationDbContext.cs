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

        // DbSets for  application entities
        public DbSet<Donation> Donations { get; set; }
        public DbSet<HelperTasks> VolunteerTasks { get; set; }
        public DbSet<Incident> Incidents { get; set; }

        // Configure table names or relationships if needed
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //  Configure Donation table
            builder.Entity<Donation>().ToTable("Donations");
            builder.Entity<HelperTasks>().ToTable("VolunteerTasks");
            builder.Entity<Incident>().ToTable("Incidents");

            
        }
    }
}
