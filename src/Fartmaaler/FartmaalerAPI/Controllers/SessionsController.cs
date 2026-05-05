using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories.Interfaces;
using FartmaalerAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FartmaalerAPI.Controllers
{
    // Denne controller håndterer HTTP requests for sessions
    // Selve logikken ligger i SessionService
    [ApiController]
    [Route("api/[controller]")]
    public class SessionsController : ControllerBase
    {
        // Repository bruges til basic crud
        private readonly IRepository<Session> _repo;

        // DbContext bruges til simple opslag
        private readonly AppDbContext _context;

        // Service bruges til forretningslogik
        private readonly SessionService _sessionService;

        // Constructor modtager dependencies via dependency injection
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

        // Henter en session ud fra id
        [HttpGet("{id}")]
        public ActionResult<Session> GetById(int id)
        {
            var session = _repo.GetById(id);

            // Hvis session ikke findes returneres 404
            if (session == null)
                return NotFound(new { message = "Session blev ikke fundet" });

            return Ok(session);
        }

        // Starter en session for en gruppe
        [HttpPost]
        public ActionResult<Session> Add(Session session)
        {
            // Validerer input
            if (session.GroupId <= 0)
                return BadRequest(new { message = "GroupId er påkrævet" });

            if (string.IsNullOrWhiteSpace(session.CarType))
                return BadRequest(new { message = "Biltype er påkrævet" });

            if (string.IsNullOrWhiteSpace(session.RoadType))
                return BadRequest(new { message = "Vejtype er påkrævet" });

            // Finder gruppen
            var group = _context.Groups.FirstOrDefault(g => g.Id == session.GroupId);

            if (group == null)
                return NotFound(new { message = "Gruppen blev ikke fundet" });

            // Tjekker om gruppen allerede er i gang
            if (group.IsLocked)
                return BadRequest(new { message = "Gruppen er allerede i gang med en session" });

            // Sætter startdata
            session.CreatedAt = DateTime.Now;
            session.Status = "Started";

            // Sætter hastighedsgrænse og scaling factor
            if (session.RoadType == "Byzone 50")
            {
                session.SpeedLimit = 50;
                session.ScalingFactor = 10;
            }
            else if (session.RoadType == "Landevej 80")
            {
                session.SpeedLimit = 80;
                session.ScalingFactor = 15;
            }
            else if (session.RoadType == "Motorvej 130")
            {
                session.SpeedLimit = 130;
                session.ScalingFactor = 20;
            }
            else
            {
                return BadRequest(new { message = "Ugyldig vejtype" });
            }

            // Låser gruppen
            group.IsLocked = true;

            // Gemmer session
            var created = _repo.Add(session);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // Afslutter en session
        [HttpPut("{id}/end")]
        public ActionResult<Session> EndSession(int id)
        {
            // Tjekker om session findes
            var existing = _context.Sessions.FirstOrDefault(s => s.Id == id);

            if (existing == null)
                return NotFound(new { message = "Session blev ikke fundet" });

            // Kalder service som håndterer logikken
            var endedSession = _sessionService.EndSession(id);

            return Ok(endedSession);
        }

        // Henter historik for en gruppe med filtrering
        [HttpGet("group/{groupId}/history")]
        public IActionResult GetHistoryByGroup(
            int groupId,
            string? carType,
            string? roadType,
            DateTime? startDate,
            DateTime? endDate)
        {
            // Kalder service som laver filtrering og beregninger
            var history = _sessionService.GetHistoryByGroup(
                groupId,
                carType,
                roadType,
                startDate,
                endDate);

            // Hvis gruppen ikke findes returneres 404
            if (history == null)
                return NotFound(new { message = "Gruppen blev ikke fundet" });

            return Ok(history);
        }

        // Opdaterer en session
        [HttpPut("{id}")]
        public ActionResult<Session> Update(int id, Session session)
        {
            var updated = _repo.Update(id, session);

            if (updated == null)
                return NotFound(new { message = "Session blev ikke fundet" });

            return Ok(updated);
        }

        // Sletter en session
        [HttpDelete("{id}")]
        public ActionResult<Session> Delete(int id)
        {
            var deleted = _repo.Delete(id);

            if (deleted == null)
                return NotFound(new { message = "Session blev ikke fundet" });

            return Ok(deleted);
        }
    }
}