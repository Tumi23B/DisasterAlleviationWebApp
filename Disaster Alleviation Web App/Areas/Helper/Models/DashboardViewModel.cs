using System.Collections.Generic;
using Disaster_Alleviation_Web_App.Models;

namespace Disaster_Alleviation_Web_App.Areas.Helper.Models
{
    public class DashboardViewModel
    {
        public int TotalAssignedTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public List<HelperTasks> RecentTasks { get; set; }
        public string HelperName { get; set; }
        public string Skills { get; set; }
        public string Availability { get; set; }
    }
}