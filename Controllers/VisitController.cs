using Microsoft.AspNetCore.Mvc;
using Backend_Mboagreen.Data;
using Backend_Mboagreen.Models;


namespace Backend_Mboagreen.Controllers
{

[Route("api/[controller]")]
[ApiController]

public class VisitController : ControllerBase
{

    private readonly AppDbContext _context;


    public VisitController(AppDbContext context)
    {
        _context = context;
    }



    [HttpPost]
    public async Task<IActionResult> AddVisit()
    {

        var visit = new VisitModel
        {
            VisitDate = DateTime.Now,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        };


        _context.Visits.Add(visit);

        await _context.SaveChangesAsync();


        return Ok(new {
            message = "Visite enregistrée"
        });

    }

}

}