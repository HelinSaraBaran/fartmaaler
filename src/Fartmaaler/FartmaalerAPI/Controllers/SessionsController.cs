using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories.Interfaces;
using FartmaalerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FartmaalerAPI.DTOs;

namespace FartmaalerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionsController : ControllerBase
    {
        private readonly IRepository<Session> _repo;
        private readonly AppDbContext _context;
        private readonly SessionService _sessionService;

        // Constructor modtager repository context og service
        public SessionsController(
            IRepository<Session> repo,
            AppDbContext context,
            SessionService sessionService)
        {
            _repo = repo;
            _context = context;
            _sessionService = sessionService;
        }

        // Henter alle sessions
        [HttpGet]
        public ActionResult<IEnumerable<Session>> GetAll()
        {
            return Ok(_repo.GetAll()); // 200
        }

        // Henter session ud fra id
        [HttpGet("{id}")]
        public ActionResult<Session> GetById(int id)
        {
            var session = _repo.GetById(id);

            if (session == null)
                return NotFound(new { message = "Session blev ikke fundet" }); // 404

            return Ok(session); // 200
        }

        // Opretter en ny session
        [HttpPost]
        public ActionResult<Session> Add([FromBody] StartSessionRequest request)
        {
            // Tjekker om group id er gyldigt
            if (request.GroupId <= 0)
                return BadRequest(new { message = "GroupId er påkrævet" }); // 400

            // Tjekker om biltype er udfyldt
            if (string.IsNullOrWhiteSpace(request.CarType))
                return BadRequest(new { message = "Biltype er påkrævet" }); // 400

            // Tjekker om vejtype er udfyldt
            if (string.IsNullOrWhiteSpace(request.RoadType))
                return BadRequest(new { message = "Vejtype er påkrævet" }); // 400

            // Tjekker om vejtypen er gyldig
            if (request.RoadType.ToLower() != "byzone 50" &&
                request.RoadType.ToLower() != "landevej 80" &&
                request.RoadType.ToLower() != "motorvej 130")
                return BadRequest(new { message = "Vejtypen er ikke gyldig" }); // 400

            // Finder gruppen i databasen
            var group = _context.Groups
                .FirstOrDefault(g => g.Id == request.GroupId);

            // Returnerer fejl hvis gruppen ikke findes
            if (group == null)
                return NotFound(new { message = "Gruppen blev ikke fundet" }); // 404

            // Tjekker om gruppen allerede har en aktiv session
            if (group.IsLocked)
                return BadRequest(new { message = "Gruppen er allerede i gang med en session" }); // 400

            // Opretter session objekt
            var session = new Session
            {
                GroupId = request.GroupId,
                CarType = request.CarType,
                RoadType = request.RoadType.ToLower(),
                SpeedLimit = _sessionService.GetSpeedLimit(request.RoadType),
                ScalingFactor = _sessionService.GetScalingFactor(request.RoadType),
                Status = "Started",
                CreatedAt = DateTime.Now
            };

            // Låser gruppen mens sessionen kører
            group.IsLocked = true;

            // Gemmer sessionen
            var created = _repo.Add(session);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created); // 201
        }

        // Afslutter en session
        [HttpPut("{id}/end")]
        public ActionResult<Session> EndSession(int id)
        {
            // Finder sessionen
            var existing = _context.Sessions
                .FirstOrDefault(s => s.Id == id);

            // Returnerer fejl hvis session ikke findes
            if (existing == null)
                return NotFound(new { message = "Session blev ikke fundet" }); // 404

            // Afslutter sessionen via service
            var endedSession = _sessionService.EndSession(id);

            return Ok(endedSession); // 200
        }

        // Henter historik for en gruppe
        [HttpGet("group/{groupId}/history")]
        public IActionResult GetHistoryByGroup(
            int groupId,
            string? carType,
            string? roadType,
            DateTime? startDate,
            DateTime? endDate)
        {
            var history = _sessionService.GetHistoryByGroup(
                groupId,
                carType,
                roadType,
                startDate,
                endDate);

            // Returnerer fejl hvis gruppen ikke findes
            if (history == null)
                return NotFound(new { message = "Gruppen blev ikke fundet" }); // 404

            return Ok(history); // 200
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public ActionResult<Session> Update(int id, Session session)
        {
            var updated = _repo.Update(id, session);

            // Returnerer fejl hvis session ikke findes
            if (updated == null)
                return NotFound(new { message = "Session blev ikke fundet" }); // 404

            return Ok(updated); // 200
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public ActionResult<Session> Delete(int id)
        {
            var deleted = _repo.Delete(id);

            // Returnerer fejl hvis session ikke findes
            if (deleted == null)
                return NotFound(new { message = "Session blev ikke fundet" }); // 404

            return Ok(deleted); // 200
        }

        // DELETE /api/sessions/all — slet alle sessions og målinger (kun admin)
        [Authorize(Roles = "admin")]
        [HttpDelete("all")]
        public IActionResult DeleteAll()
        {
            try
            {
                _context.Measurements.RemoveRange(_context.Measurements);
                _context.Sessions.RemoveRange(_context.Sessions);

                // Frigør alle grupper
                var groups = _context.Groups.Where(g => g.IsLocked).ToList();
                foreach (var group in groups)
                {
                    group.IsLocked = false;
                }

                _context.SaveChanges();
                return Ok(new { message = "Al historik er slettet" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Der opstod en fejl", error = ex.Message });
            }
        }

        // GET /api/sessions/class-summary — gennemsnit for hele klassen (kun admin)
        [Authorize(Roles = "admin")]
        [HttpGet("class-summary")]
        public IActionResult GetClassSummary()
        {
            try
            {
                var measurements = _context.Measurements.ToList();

                if (!measurements.Any())
                    return Ok(new { message = "Ingen data endnu", count = 0 });

                return Ok(new
                {
                    TotalMeasurements = measurements.Count,
                    AverageSpeed = Math.Round(measurements.Average(m => m.SimulatedSpeed), 2),
                    AverageCo2 = Math.Round(measurements.Average(m => m.Co2), 2),
                    AverageDeviation = Math.Round(measurements.Average(m => Math.Abs(m.SimulatedSpeed - m.SpeedLimit)), 2),
                    AverageScore = Math.Round(measurements.Average(m => Math.Abs(m.SimulatedSpeed - m.SpeedLimit) + m.Co2), 2),
                    TotalSessions = _context.Sessions.Count()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Der opstod en fejl", error = ex.Message });
            }
        }
    }
}