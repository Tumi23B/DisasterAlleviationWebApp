using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Disaster_Alleviation_Web_App.Areas.Donor.Models
{
    public class DashboardViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public decimal TotalDonations { get; set; }
        public int PendingRequests { get; set; }
        public int CompletedDonations { get; set; }
        public int ImpactScore { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime LastDonationDate { get; set; }
    }

    public class DonationHistoryViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string CampaignName { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string ReferenceNumber { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class HistoryViewModel
    {
        public string FullName { get; set; }
        public List<DonationHistoryViewModel> Donations { get; set; }
        public decimal TotalDonated { get; set; }
        public int TotalDonations { get; set; }
    }

    public class ActiveRecurringDonation
    {
        public int Id { get; set; }
        public string CampaignName { get; set; }
        public decimal Amount { get; set; }
        public string Frequency { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime NextPaymentDate { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class RecurringDonationViewModel
    {
        public string FullName { get; set; }
        public List<ActiveRecurringDonation> ActiveDonations { get; set; }
        public decimal TotalMonthly { get; set; }
        public decimal TotalYearly { get; set; }
    }

    public class RecurringDonationSubmission
    {
        [Required]
        [Range(1, 1000000, ErrorMessage = "Amount must be at least R1")]
        public decimal Amount { get; set; }

        [Required]
        public string Frequency { get; set; }

        [Required]
        public string Campaign { get; set; }

        [Required]
        public string PaymentMethod { get; set; }
    }
}