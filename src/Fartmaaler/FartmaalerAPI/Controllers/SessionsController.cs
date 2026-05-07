using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories.Interfaces;
using FartmaalerAPI.Services;
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

        public SessionsController(
            IRepository<Session> repo,
            AppDbContext context,
            SessionService sessionService)
        {
            _repo = repo;
            _context = context;
            _sessionService = sessionService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Session>> GetAll()
        {
            return Ok(_repo.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<Session> GetById(int id)
        {
            var session = _repo.GetById(id);

            if (session == null)
                return NotFound(new { message = "Session blev ikke fundet" });

            return Ok(session);
        }

        [HttpPost]
        public ActionResult<Session> Add(Session session)
        {
            if (session.GroupId <= 0)
                return BadRequest(new { message = "GroupId er påkrævet" });

            if (string.IsNullOrWhiteSpace(session.CarType))
                return BadRequest(new { message = "Biltype er påkrævet" });

            if (string.IsNullOrWhiteSpace(session.RoadType))
                return BadRequest(new { message = "Vejtype er påkrævet" });

            var group = _context.Groups.FirstOrDefault(g => g.Id == session.GroupId);

            if (group == null)
                return NotFound(new { message = "Gruppen blev ikke fundet" });

            if (group.IsLocked)
                return BadRequest(new { message = "Gruppen er allerede i gang med en session" });

            session.CreatedAt = DateTime.Now;
            session.Status = "Started";

            if (session.RoadType?.ToLower() == "byzone 50")
            {
                session.SpeedLimit = 50;
                session.ScalingFactor = 10;
            }
            else if (session.RoadType?.ToLower() == "landevej 80")
            {
                session.SpeedLimit = 80;
                session.ScalingFactor = 15;
            }
            else if (session.RoadType?.ToLower() == "motorvej 130")
            {
                session.SpeedLimit = 130;
                session.ScalingFactor = 20;
            }
            else
            {
                return BadRequest(new { message = "Ugyldig vejtype" });
            }

            group.IsLocked = true;

            var created = _repo.Add(session);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}/end")]
        public ActionResult<Session> EndSession(int id)
        {
            var existing = _context.Sessions.FirstOrDefault(s => s.Id == id);

            if (existing == null)
                return NotFound(new { message = "Session blev ikke fundet" });

            var endedSession = _sessionService.EndSession(id);

            return Ok(endedSession);
        }

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

            if (history == null)
                return NotFound(new { message = "Gruppen blev ikke fundet" });

            return Ok(history);
        }

        [HttpPut("{id}")]
        public ActionResult<Session> Update(int id, Session session)
        {
            var updated = _repo.Update(id, session);

            if (updated == null)
                return NotFound(new { message = "Session blev ikke fundet" });

            return Ok(updated);
        }

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