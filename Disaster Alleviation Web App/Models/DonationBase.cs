using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disaster_Alleviation_Web_App.Models
{
    public class DonationBase
    {

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime DonationDate { get; set; }

        [Required]
        public DateTime DonationDate { get; set; } = DateTime.Now;
        [Key]
        public int DonationId { get; set; }
        [Key]
        public int DonationId { get; set; }

        [Required]
        [StringLength(50)]
        public string DonationType { get; set; } // Money, Goods, Services

        [StringLength(100)]
        public string DonationType { get; set; } // "Money", "Goods", "Services"

        [ForeignKey("DonorId")]
        public ApplicationUser Donor { get; set; }

        // Navigation property
        [ForeignKey("DonorId")]
        public virtual ApplicationUser Donor { get; set; }

        [Required]
        public string DonorId { get; set; }

        [Required]
        public string DonorId { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } // Pending, Completed, Cancelled

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // "Pending", "Completed", "Cancelled"
    }
}