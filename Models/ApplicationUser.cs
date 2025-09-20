using System;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Models;

public class ApplicationUser : IdentityUser
{
    // Additional properties can be added here if needed
    // For example, you can add FirstName, LastName, etc.
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
