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
            return Ok(_repo.GetAll());
        }

        // Henter session ud fra id
        [HttpGet("{id}")]
        public ActionResult<Session> GetById(int id)
        {
            Session? session = _repo.GetById(id);

            if (session == null)
                return NotFound(new { message = "Session blev ikke fundet" });

            return Ok(session);
        }

        // Opretter en ny session
        [HttpPost]
        public ActionResult<Session> Add([FromBody] StartSessionRequest request)
        {
            // Tjekker om group id er gyldigt
            if (request.GroupId <= 0)
                return BadRequest(new { message = "GroupId er påkrævet" });

            // Tjekker om biltype er udfyldt
            if (string.IsNullOrWhiteSpace(request.CarType))
                return BadRequest(new { message = "Biltype er påkrævet" });

            // Tjekker om vejtype er udfyldt
            if (string.IsNullOrWhiteSpace(request.RoadType))
                return BadRequest(new { message = "Vejtype er påkrævet" });

            // Tjekker om vejtypen er gyldig
            if (request.RoadType.ToLower() != "byzone 50" &&
                request.RoadType.ToLower() != "landevej 80" &&
                request.RoadType.ToLower() != "motorvej 130")
                return BadRequest(new { message = "Vejtypen er ikke gyldig" });

            // Finder gruppen i databasen
            Group? group = _context.Groups
                .FirstOrDefault(group => group.Id == request.GroupId);

            if (group == null)
                return NotFound(new { message = "Gruppen blev ikke fundet" });

            // Tjekker om gruppen allerede har en aktiv session
            if (group.IsLocked)
                return BadRequest(new { message = "Gruppen er allerede i gang med en session" });

            // Opretter session objekt
            Session session = new Session
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

            Session created = _repo.Add(session);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // Afslutter en session
        [HttpPut("{id}/end")]
        public ActionResult<Session> EndSession(int id)
        {
            Session? existing = _context.Sessions
                .FirstOrDefault(session => session.Id == id);

            if (existing == null)
                return NotFound(new { message = "Session blev ikke fundet" });

            Session? endedSession = _sessionService.EndSession(id);

            return Ok(endedSession);
        }

        // Henter historik for en gruppe med filter og sortering
        [HttpGet("group/{groupId}/history")]
        public IActionResult GetHistoryByGroup(
            int groupId,
            string? carType,
            string? roadType,
            DateTime? startDate,
            DateTime? endDate,
            string? sortBy,
            string? sortDirection)
        {
            object? history = _sessionService.GetHistoryByGroup(
                groupId,
                carType,
                roadType,
                startDate,
                endDate,
                sortBy,
                sortDirection);

            if (history == null)
                return NotFound(new { message = "Gruppen blev ikke fundet" });

            return Ok(history);
        }

        // Opdaterer en session
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public ActionResult<Session> Update(int id, Session session)
        {
            Session? updated = _repo.Update(id, session);

            if (updated == null)
                return NotFound(new { message = "Session blev ikke fundet" });

            return Ok(updated);
        }

        // Sletter en session
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public ActionResult<Session> Delete(int id)
        {
            Session? deleted = _repo.Delete(id);

            if (deleted == null)
                return NotFound(new { message = "Session blev ikke fundet" });

            return Ok(deleted);
        }

        // Sletter alle sessions og målinger
        [Authorize(Roles = "admin")]
        [HttpDelete("all")]
        public IActionResult DeleteAll()
        {
            try
            {
                _context.Measurements.RemoveRange(_context.Measurements);
                _context.Sessions.RemoveRange(_context.Sessions);

                // Frigør alle grupper
                List<Group> groups = _context.Groups
                    .Where(group => group.IsLocked)
                    .ToList();

                foreach (Group group in groups)
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

        // Henter gennemsnit for hele klassen
        [Authorize(Roles = "admin")]
        [HttpGet("class-summary")]
        public IActionResult GetClassSummary()
        {
            try
            {
                List<Measurement> measurements = _context.Measurements.ToList();

                if (!measurements.Any())
                    return Ok(new { message = "Ingen data endnu", count = 0 });

                return Ok(new
                {
                    TotalMeasurements = measurements.Count,
                    AverageSpeed = Math.Round(measurements.Average(measurement => measurement.SimulatedSpeed), 2),
                    AverageCo2 = Math.Round(measurements.Average(measurement => measurement.Co2), 2),
                    AverageDeviation = Math.Round(measurements.Average(measurement => Math.Abs(measurement.SimulatedSpeed - measurement.SpeedLimit)), 2),
                    AverageScore = Math.Round(measurements.Average(measurement => Math.Abs(measurement.SimulatedSpeed - measurement.SpeedLimit) + measurement.Co2), 2),
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