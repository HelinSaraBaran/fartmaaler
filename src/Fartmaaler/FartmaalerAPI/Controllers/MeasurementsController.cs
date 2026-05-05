using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FartmaalerAPI.Controllers
{
    // Denne controller håndterer målinger i systemet
    // Målinger bruges til historik, live oversigt og leaderboard
    [ApiController]
    [Route("api/[controller]")]
    public class MeasurementsController : ControllerBase
    {
        // Repository bruges til basic CRUD
        private readonly IRepository<Measurement> _repo;

        // DbContext bruges til opslag på tværs af tabeller
        private readonly AppDbContext _context;

        // Constructor modtager dependencies via dependency injection
        public MeasurementsController(IRepository<Measurement> repo, AppDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        // Henter alle målinger
        [HttpGet]
        public ActionResult<IEnumerable<Measurement>> GetAll()
        {
            return Ok(_repo.GetAll());
        }

        // Henter en måling ud fra id
        [HttpGet("{id}")]
        public ActionResult<Measurement> GetById(int id)
        {
            var measurement = _repo.GetById(id);

            // Hvis måling ikke findes returneres 404
            if (measurement == null)
                return NotFound(new { message = "Måling blev ikke fundet" });

            return Ok(measurement);
        }

        // Opretter en ny måling
        // Bruges når sensor eller gruppe sender data
        [HttpPost]
        public ActionResult<Measurement> Add(Measurement measurement)
        {
            // Tjekker at sessionId er valid
            if (measurement.SessionId <= 0)
                return BadRequest(new { message = "SessionId er påkrævet" });

            // Finder sessionen
            var session = _context.Sessions.FirstOrDefault(s => s.Id == measurement.SessionId);

            if (session == null)
                return NotFound(new { message = "Session blev ikke fundet" });

            // Må ikke tilføje målinger til afsluttet session
            if (session.Status == "Ended")
                return BadRequest(new { message = "Der kan ikke tilføjes målinger til en afsluttet session" });

            // Sætter tidspunkt for målingen
            measurement.CreatedAt = DateTime.Now;

            // Gemmer målingen
            var created = _repo.Add(measurement);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // Sletter en måling
        [HttpDelete("{id}")]
        public ActionResult<Measurement> Delete(int id)
        {
            var deleted = _repo.Delete(id);

            if (deleted == null)
                return NotFound(new { message = "Måling blev ikke fundet" });

            return Ok(deleted);
        }

        // Målinger må ikke opdateres
        [HttpPut("{id}")]
        public IActionResult Update(int id, Measurement measurement)
        {
            return BadRequest(new { message = "Målinger kan ikke opdateres" });
        }

        // Live oversigt for underviser
        // Viser alle grupper og deres seneste måling
        // Viser også status hvis gruppen ikke har målt endnu
        [Authorize(Roles = "Teacher")]
        [HttpGet("live-overview")]
        public IActionResult GetLiveOverview()
        {
            // Henter alle grupper
            var result = _context.Groups
                .Select(g => new
                {
                    GroupId = g.Id,
                    GroupName = g.Name,
                    School = g.School,
                    IsLocked = g.IsLocked,

                    // Finder gruppens nyeste session
                    LatestSession = _context.Sessions
                        .Where(s => s.GroupId == g.Id)
                        .OrderByDescending(s => s.CreatedAt)
                        .Select(s => new
                        {
                            s.Id,
                            s.CarType,
                            s.RoadType,
                            s.SpeedLimit,
                            s.Status,
                            s.CreatedAt,
                            s.EndedAt
                        })
                        .FirstOrDefault(),

                    // Finder gruppens nyeste måling
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
                        .FirstOrDefault(),

                    // Viser status til frontend
                    Status = _context.Measurements
                        .Any(m => _context.Sessions
                            .Any(s => s.Id == m.SessionId && s.GroupId == g.Id))
                            ? "Måling registreret"
                            : "Venter på måling"
                })
                .ToList();

            return Ok(result);
        }

        // Leaderboard
        // Viser grupper sorteret efter bedste hastighed
        [HttpGet("leaderboard")]
        public IActionResult GetLeaderboard()
        {
            var result = _context.Groups
                .Select(g => new
                {
                    GroupId = g.Id,
                    GroupName = g.Name,

                    // Finder højeste hastighed for gruppen
                    BestSpeed = _context.Measurements
                        .Where(m => _context.Sessions
                            .Any(s => s.Id == m.SessionId && s.GroupId == g.Id))
                        .Max(m => (double?)m.SimulatedSpeed),

                    // Tæller antal målinger
                    Count = _context.Measurements
                        .Count(m => _context.Sessions
                            .Any(s => s.Id == m.SessionId && s.GroupId == g.Id))
                })
                .Where(x => x.BestSpeed != null)
                .OrderByDescending(x => x.BestSpeed)
                .ToList();

            if (!result.Any())
                return NotFound(new { message = "Ingen data til leaderboard" });

            return Ok(result);
        }

        // Henter alle målinger for en session
        [HttpGet("session/{sessionId}")]
        public IActionResult GetMeasurementsBySession(int sessionId)
        {
            // Tjekker om session findes
            var session = _context.Sessions.FirstOrDefault(s => s.Id == sessionId);

            if (session == null)
                return NotFound(new { message = "Session blev ikke fundet" });

            // Henter målinger sorteret nyeste først
            var measurements = _context.Measurements
                .Where(m => m.SessionId == sessionId)
                .OrderByDescending(m => m.CreatedAt)
                .ToList();

            return Ok(measurements);
        }

        // Henter opsummering for en session
        // Bruges efter session er afsluttet
        [HttpGet("session/{sessionId}/summary")]
        public IActionResult GetSessionSummary(int sessionId)
        {
            // Finder sessionen
            var session = _context.Sessions.FirstOrDefault(s => s.Id == sessionId);

            if (session == null)
                return NotFound(new { message = "Session blev ikke fundet" });

            // Henter alle målinger
            var measurements = _context.Measurements
                .Where(m => m.SessionId == sessionId)
                .ToList();

            // Hvis ingen målinger returneres 0 værdier
            if (!measurements.Any())
                return Ok(new
                {
                    Count = 0,
                    AverageSpeed = 0,
                    AverageCo2 = 0,
                    TotalCo2Saved = 0
                });

            // Beregner opsummering
            var result = new
            {
                Count = measurements.Count,
                AverageSpeed = measurements.Average(m => m.SimulatedSpeed),
                AverageCo2 = measurements.Average(m => m.Co2),
                TotalCo2Saved = measurements.Sum(m => m.Co2Saved)
            };

            return Ok(result);
        }
    }
}