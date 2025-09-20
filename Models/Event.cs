using System;

namespace ExpenseTracker.Models;

public enum PaymentMethod { Cash = 1, ETransfer = 2, Credit = 3, Other = 99 }
public enum EventStatus { Open = 1, Closed = 2 }

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime Date { get; set; }
    public decimal TotalCost { get; set; }                 // excel: Total Cost
    public int ConsumersCount { get; set; }                // people included in split
    public string? Notes { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Open;

    // Snapshots when closing (optional but handy in UI)
    public decimal? PerPersonAmount { get; set; }
    public decimal? SurplusOrDeficit { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
