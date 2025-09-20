using ExpenseTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FundController : ControllerBase
{
    private readonly EventService _svc;
    public FundController(EventService svc) => _svc = svc;

    [HttpGet("balance")]
    public ActionResult<decimal> GetBalance() => _svc.GetFundBalance();
}
