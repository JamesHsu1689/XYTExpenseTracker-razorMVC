using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services;

public record EventDto(int Id, string Name, DateTime Date, decimal TotalCost, int ConsumersCount, string? Notes, string Status);
public record PaymentRowDto(int Id, decimal Amount, string Method, string? Notes);
public record EventDetailsDto(EventDto Event, decimal PerPerson, decimal TotalCollected, decimal Surplus, List<PaymentRowDto> Payments);
public record CreateEventDto(string Name, DateTime Date, decimal TotalCost, int ConsumersCount, string? Notes);
public record AddPaymentDto(decimal Amount, string Method, string? Notes);

public class EventService
{
    private readonly ApplicationDbContext _db;
    public EventService(ApplicationDbContext db) => _db = db;

    public async Task<List<EventDto>> GetEventsAsync()
    {
        return await _db.Events
            .OrderByDescending(e => e.Date)
            .Select(e => new EventDto(
                e.Id, e.Name, e.Date, e.TotalCost, e.ConsumersCount, e.Notes, e.Status.ToString()))
            .ToListAsync();
    }

    public async Task<EventDetailsDto> GetEventAsync(int id)
    {
        var e = await _db.Events.Include(x => x.Payments).FirstAsync(x => x.Id == id);
        var perPerson = e.ConsumersCount > 0
            ? Math.Round(e.TotalCost / e.ConsumersCount, 2, MidpointRounding.AwayFromZero) : 0m;
        var totalCollected = e.Payments.Sum(p => p.Amount);
        var surplus = Math.Round(totalCollected - e.TotalCost, 2, MidpointRounding.AwayFromZero);

        return new EventDetailsDto(
            new EventDto(e.Id, e.Name, e.Date, e.TotalCost, e.ConsumersCount, e.Notes, e.Status.ToString()),
            perPerson,
            totalCollected,
            surplus,
            e.Payments.OrderBy(p => p.Id)
                      .Select(p => new PaymentRowDto(p.Id, p.Amount, p.Method.ToString(), p.Notes))
                      .ToList()
        );
    }

    public async Task<int> CreateEventAsync(CreateEventDto dto)
    {
        var e = new Event
        {
            Name = dto.Name, Date = dto.Date, TotalCost = dto.TotalCost,
            ConsumersCount = dto.ConsumersCount, Notes = dto.Notes
        };
        _db.Events.Add(e);
        await _db.SaveChangesAsync();
        return e.Id;
    }

    public async Task AddPaymentAsync(int eventId, AddPaymentDto dto)
    {
        var e = await _db.Events.FirstAsync(x => x.Id == eventId);
        var method = Enum.TryParse<PaymentMethod>(dto.Method, out var m) ? m : PaymentMethod.Other;
        _db.Payments.Add(new Payment { EventId = e.Id, Amount = dto.Amount, Method = m, Notes = dto.Notes });
        await _db.SaveChangesAsync();
    }

    public async Task CloseEventAndPostSurplusAsync(int id)
    {
        var e = await _db.Events.Include(x => x.Payments).FirstAsync(x => x.Id == id);

        var perPerson = e.ConsumersCount > 0
            ? Math.Round(e.TotalCost / e.ConsumersCount, 2, MidpointRounding.AwayFromZero) : 0m;
        var totalCollected = e.Payments.Sum(p => p.Amount);
        var surplus = Math.Round(totalCollected - e.TotalCost, 2, MidpointRounding.AwayFromZero);

        e.PerPersonAmount = perPerson;
        e.SurplusOrDeficit = surplus;
        e.Status = EventStatus.Closed;

        if (surplus != 0m)
            _db.FundTransactions.Add(new FundTransaction
            {
                Amount = surplus,
                EventId = e.Id,
                Notes = $"Close event '{e.Name}'"
            });

        await _db.SaveChangesAsync();
    }

    public decimal GetFundBalance()
        => Math.Round(_db.FundTransactions.Sum(t => t.Amount), 2, MidpointRounding.AwayFromZero);
}
