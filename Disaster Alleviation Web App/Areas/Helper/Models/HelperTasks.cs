using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disaster_Alleviation_Web_App.Models
{
    public class HelperTasks
    {
        [Key]
        public int TaskId { get; set; }

        [Required]
        public string HelperId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime AssignedDate { get; set; } = DateTime.Now;

        public DateTime? CompletedDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Assigned";

        [Required]
        [StringLength(100)]
        public string TaskType { get; set; }

        public int Priority { get; set; } = 1;

        // Navigation property
        [ForeignKey("HelperId")]
        public virtual ApplicationUser Helper { get; set; }
    }
}