using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disaster_Alleviation_Web_App.Models
{
    public class Donation
    {
        [Key]
        public int DonationId { get; set; }

        [Required]
        public string DonorId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime DonationDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // "Pending", "Completed", "Cancelled"

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(100)]
        public string DonationType { get; set; } // "Money", "Goods", "Services"

        // Navigation property
        [ForeignKey("DonorId")]
        public virtual ApplicationUser Donor { get; set; }
    }
}