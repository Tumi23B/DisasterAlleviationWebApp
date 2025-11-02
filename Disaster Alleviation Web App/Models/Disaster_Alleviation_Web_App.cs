using System;
using Microsoft.AspNetCore.Identity;

namespace Disaster_Alleviation_Web_App.Models;

// Identity User


// Donation Entity
public class Donations
{
    public int Id { get; set; } // Primary Key
    public string DonorName { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } // Optional
}

// Volunteer Task Entity
public class HelperTask

{
    public int Id { get; set; } // Primary Key
    public string TaskName { get; set; }
    public string AssignedTo { get; set; } // Could be a user's Id
    public DateTime AssignedDate { get; set; }
    public bool IsCompleted { get; set; } = false;
}

// Incident or Disaster Entity
public class Incident
{
    public int Id { get; set; } // Primary Key
    public string Title { get; set; }
    public string Location { get; set; }
    public DateTime DateOccurred { get; set; }
    public string Description { get; set; }
}
