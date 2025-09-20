using System;

namespace ExpenseTracker.Models;

public class FundTransaction
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // + increases fund (surplus/donations), - decreases fund (used to pay)
    public decimal Amount { get; set; }
    public int? EventId { get; set; }
    public string? Notes { get; set; }
}
