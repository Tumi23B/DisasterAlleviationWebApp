using Disaster_Alleviation_Web_App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Disaster_Alleviation_Web_App.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1️⃣ Create roles
            string[] roles = { "Admin", "Donor", "Helper" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // 2️⃣ Create admin user
            var adminEmail = "admin@example.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true,

                    // Fill all required fields
                    FullName = "Administrator",
                    Skills = "Management",
                    Availability = "Full-Time",
                    CellphoneNumber = "0000000000",
                    Address = "Admin HQ"
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123"); // Strong password
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
