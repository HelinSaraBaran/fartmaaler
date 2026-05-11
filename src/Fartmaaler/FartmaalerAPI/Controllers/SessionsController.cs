using FartmaalerAPI.Data;
using FartmaalerAPI.DTOs;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories.Interfaces;
using FartmaalerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FartmaalerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionsController : ControllerBase
    {
        private readonly IRepository<Session> _repo;
        private readonly AppDbContext _context;
        private readonly SessionService _sessionService;

        // Constructor modtager repository, context og service
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
            {
                return NotFound(new { message = "Session blev ikke fundet" });
            }

            return Ok(session);
        }

        // Opretter en ny session
        [HttpPost]
        public ActionResult<Session> Add([FromBody] StartSessionRequest request)
        {
            // Tjekker om group id er gyldigt
            if (request.GroupId <= 0)
            {
                return BadRequest(new { message = "GroupId er påkrævet" });
            }

            // Tjekker om biltype er udfyldt
            if (string.IsNullOrWhiteSpace(request.CarType))
            {
                return BadRequest(new { message = "Biltype er påkrævet" });
            }

            // Tjekker om vejtype er udfyldt
            if (string.IsNullOrWhiteSpace(request.RoadType))
            {
                return BadRequest(new { message = "Vejtype er påkrævet" });
            }

            string roadType = request.RoadType.Trim().ToLower();

            // Tjekker om vejtypen er gyldig
            if (roadType != "byzone 50" &&
                roadType != "landevej 80" &&
                roadType != "motorvej 130")
            {
                return BadRequest(new { message = "Vejtypen er ikke gyldig" });
            }

            // Finder gruppen først, så vi kan give en præcis fejlbesked
            Group? group = _context.Groups
                .FirstOrDefault(group => group.Id == request.GroupId);

            if (group == null)
            {
                return NotFound(new { message = "Gruppen blev ikke fundet" });
            }

            if (group.IsLocked)
            {
                return BadRequest(new { message = "Gruppen er allerede i gang med en session" });
            }

            // Service opretter sessionen og låser gruppen
            Session? createdSession = _sessionService.StartSession(
                request.GroupId,
                request.CarType.Trim(),
                roadType);

            if (createdSession == null)
            {
                return BadRequest(new { message = "Session kunne ikke startes" });
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdSession.Id },
                createdSession);
        }

        // Afslutter en session
        [HttpPut("{id}/end")]
        public ActionResult<Session> EndSession(int id)
        {
            Session? endedSession = _sessionService.EndSession(id);

            if (endedSession == null)
            {
                return NotFound(new { message = "Session blev ikke fundet" });
            }

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
            {
                return NotFound(new { message = "Gruppen blev ikke fundet" });
            }

            return Ok(history);
        }

        // Opdaterer en session
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public ActionResult<Session> Update(int id, Session session)
        {
            Session? updated = _repo.Update(id, session);

            if (updated == null)
            {
                return NotFound(new { message = "Session blev ikke fundet" });
            }

            return Ok(updated);
        }

        // Sletter en session
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public ActionResult<Session> Delete(int id)
        {
            Session? session = _context.Sessions
                .FirstOrDefault(session => session.Id == id);

            if (session == null)
            {
                return NotFound(new { message = "Session blev ikke fundet" });
            }

            Group? group = _context.Groups
                .FirstOrDefault(group => group.Id == session.GroupId);

            if (group != null)
            {
                group.IsLocked = false;
            }

            Session? deleted = _repo.Delete(id);

            if (deleted == null)
            {
                return NotFound(new { message = "Session blev ikke fundet" });
            }

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
                {
                    return Ok(new { message = "Ingen data endnu", count = 0 });
                }

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