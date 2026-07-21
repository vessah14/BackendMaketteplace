using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend_Mboagreen.Data;

namespace Backend_Mboagreen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            var totalUsers = await _context.Users.CountAsync();

            var totalAnnonces = await _context.Annonce.CountAsync();

            return Ok(new
            {
                totalUsers,
                totalAnnonces
            });
        }
        [HttpGet("users-evolution")]
        public async Task<IActionResult> GetUsersEvolution()
        {
            var data = await _context.Users
                .GroupBy(u => new
                {
                    Year = u.CreatedAt.Year,
                    Month = u.CreatedAt.Month
                })
                .Select(g => new
                {
                    mois = g.Key.Month,
                    annee = g.Key.Year,
                    nombre = g.Count()
                })
                .OrderBy(x => x.annee)
                .ThenBy(x => x.mois)
                .ToListAsync();


            return Ok(data);
        }
        [HttpGet("visits-evolution")]

        public async Task<IActionResult> GetVisitsEvolution()
        {

            var data = await _context.Visits

            .GroupBy(v => new
            {
                Year = v.VisitDate.Year,
                Month = v.VisitDate.Month
            })

            .Select(g => new
            {

                mois = g.Key.Month,

                annee = g.Key.Year,

                nombre = g.Count()

            })

            .OrderBy(x => x.annee)
            .ThenBy(x => x.mois)

            .ToListAsync();


            return Ok(data);

        }
    }
}