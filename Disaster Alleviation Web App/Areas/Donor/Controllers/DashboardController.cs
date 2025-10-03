using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Disaster_Alleviation_Web_App.Models;
using Disaster_Alleviation_Web_App.Areas.Donor.Models;

namespace Disaster_Alleviation_Web_App.Areas.Donor.Controllers
{
    [Area("Donor")]
    [Authorize(Roles = "Donor")]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var model = new DashboardViewModel
            {
                FullName = "Valued Donor",
                Email = "donor@example.com",
                JoinDate = DateTime.Now.AddYears(-1),
                //LastLoginDate = DateTime.Now.AddDays(-1),
                TotalDonations = 50000m,
                CompletedDonations = 12,
                PendingRequests = 3
            };

            return View(model);
        }

        public IActionResult Profile()
        {
            //fetch  ApplicationUser from _userManager
            var user = _userManager.GetUserAsync(User).Result;

            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            return View(user);
        }
    }
}
