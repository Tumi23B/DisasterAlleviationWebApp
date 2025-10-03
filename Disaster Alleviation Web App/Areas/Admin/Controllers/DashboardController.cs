using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Disaster_Alleviation_Web_App.Models;
using Microsoft.AspNetCore.Identity;
using Disaster_Alleviation_Web_App.Areas.Admin.Models;

namespace Disaster_Alleviation_Web_App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();

            var model = new DashboardViewModel
            {
                TotalUsers = users.Count,
                TotalDonors = (await _userManager.GetUsersInRoleAsync("Donor")).Count,
                TotalHelpers = (await _userManager.GetUsersInRoleAsync("Helper")).Count
            };

            return View(model);
        }

    }
}
