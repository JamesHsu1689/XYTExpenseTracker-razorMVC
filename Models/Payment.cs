using System;

namespace ExpenseTracker.Models;

public class Payment
{
    public int Id { get; set; }

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public decimal Amount { get; set; }                    // excel: Amount
    public PaymentMethod Method { get; set; }              // excel: Method
    public string? Notes { get; set; }                     // excel: Notes
}
