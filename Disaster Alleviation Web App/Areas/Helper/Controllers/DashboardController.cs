using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Disaster_Alleviation_Web_App.Models;
using Microsoft.AspNetCore.Identity;
using Disaster_Alleviation_Web_App.Areas.Helper.Models;

namespace Disaster_Alleviation_Web_App.Areas.Helper.Controllers
{
    [Area("Helper")]
    [Authorize(Roles = "Helper")]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            // Fetch helper-specific data from DB
            var model = new DashboardViewModel
            {
                TotalAssignedTasks = 8,  
                CompletedTasks = 5,     
                PendingTasks = 3         
            };

            return View(model);
        }
    }
}
