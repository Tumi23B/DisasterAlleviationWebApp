using Microsoft.AspNetCore.Mvc;

namespace Disaster_Alleviation_Web_App.Areas.Helper.Controllers
{
    public class IncidentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
