using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FartmaalerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeasurementsController : ControllerBase
    {
        private readonly IRepository<Measurement> _repo;
        private readonly AppDbContext _context;

        public MeasurementsController(IRepository<Measurement> repo, AppDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Measurement>> GetAll()
        {
            return Ok(_repo.GetAll()); // 200
        }

        [HttpGet("{id}")]
        public ActionResult<Measurement> GetById(int id)
        {
            var measurement = _repo.GetById(id);

            if (measurement == null)
                return NotFound(new { message = "Måling blev ikke fundet" }); // 404

            return Ok(measurement); // 200
        }

        // Gruppen/sensoren opretter målinger
        [HttpPost]
        public ActionResult<Measurement> Add(Measurement measurement)
        {
            if (measurement.SessionId <= 0)
                return BadRequest(new { message = "SessionId er påkrævet" }); // 400

            measurement.CreatedAt = DateTime.Now;

            var created = _repo.Add(measurement);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                created
            ); // 201
        }

        [HttpDelete("{id}")]
        public ActionResult<Measurement> Delete(int id)
        {
            var deleted = _repo.Delete(id);

            if (deleted == null)
                return NotFound(new { message = "Måling blev ikke fundet" }); // 404

            return Ok(deleted); // 200
        }

        // Målinger må ikke opdateres
        [HttpPut("{id}")]
        public IActionResult Update(int id, Measurement measurement)
        {
            return BadRequest(new { message = "Målinger kan ikke opdateres" }); // 400
        }

        // User story: live oversigt
        [Authorize(Roles = "Teacher")]
        [HttpGet("live-overview")]
        public IActionResult GetLiveOverview()
        {
            var result = _context.Groups
                .Select(g => new
                {
                    GroupId = g.Id,
                    GroupName = g.Name,
                    School = g.School,

                    LatestMeasurement = _context.Measurements
                        .Where(m => _context.Sessions
                            .Any(s => s.Id == m.SessionId && s.GroupId == g.Id))
                        .OrderByDescending(m => m.CreatedAt)
                        .Select(m => new
                        {
                            m.Id,
                            m.SessionId,
                            m.MeasuredSpeed,
                            m.SimulatedSpeed,
                            m.Time,
                            m.Distance,
                            m.Co2,
                            m.Co2Saved,
                            m.CreatedAt
                        })
                        .FirstOrDefault()
                })
                .ToList();

            return Ok(result); // 200
        }

        // User story: leaderboard
        [HttpGet("leaderboard")]
        public IActionResult GetLeaderboard()
        {
            var result = _context.Groups
                .Select(g => new
                {
                    GroupId = g.Id,
                    GroupName = g.Name,
                    School = g.School,

                    BestSpeed = _context.Measurements
                        .Where(m => _context.Sessions
                            .Any(s => s.Id == m.SessionId && s.GroupId == g.Id))
                        .Max(m => (double?)m.SimulatedSpeed),

                    MeasurementCount = _context.Measurements
                        .Count(m => _context.Sessions
                            .Any(s => s.Id == m.SessionId && s.GroupId == g.Id))
                })
                .Where(x => x.BestSpeed != null)
                .OrderByDescending(x => x.BestSpeed)
                .ToList();

            if (!result.Any())
                return NotFound(new { message = "Ingen data til leaderboard" }); // 404

            return Ok(result); // 200
        }
    }
}