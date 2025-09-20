using ExpenseTracker;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize] // must be logged in; tighten with roles later
public class EventsController : ControllerBase
{
    private readonly EventService _svc;
    public EventsController(EventService svc) => _svc = svc;

    [HttpGet]
    public Task<List<EventDto>> GetAll() => _svc.GetEventsAsync();

    [HttpGet("{id:int}")]
    public Task<EventDetailsDto> Get(int id) => _svc.GetEventAsync(id);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<int> Create([FromBody] CreateEventDto dto) => _svc.CreateEventAsync(dto);

    [HttpPost("{id:int}/payments")]
    [ValidateAntiForgeryToken]
    public Task AddPayment(int id, [FromBody] AddPaymentDto dto) => _svc.AddPaymentAsync(id, dto);

    [HttpPost("{id:int}/close")]
    [ValidateAntiForgeryToken]
    public Task Close(int id) => _svc.CloseEventAndPostSurplusAsync(id);
}
