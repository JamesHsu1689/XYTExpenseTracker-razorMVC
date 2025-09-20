using System;

namespace ExpenseTracker.Models;

public class Person
{
    public int personId { get; set; }
    public required string firstName { get; set; }
    public required string lastName { get; set; }
}
