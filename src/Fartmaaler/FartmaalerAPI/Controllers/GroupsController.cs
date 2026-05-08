using FartmaalerAPI.Data;
using FartmaalerAPI.DTOs;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FartmaalerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : ControllerBase
    {
        private readonly IRepository<Group> _repo;
        private readonly AppDbContext _context;

        public GroupsController(IRepository<Group> repo, AppDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        // Bruges til dropdown
        [HttpGet]
        public ActionResult<IEnumerable<Group>> GetAll()
        {
            return Ok(_repo.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<Group> GetById(int id)
        {
            var group = _repo.GetById(id);

            if (group == null)
                return NotFound(new { message = "Gruppen blev ikke fundet" });

            return Ok(group);
        }

        // Kun lærer/user må oprette grupper
        [Authorize(Roles = "admin")]
        [HttpPost]
        public ActionResult<Group> Add([FromBody] CreateGroupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Gruppens navn skal udfyldes" });

            string name = request.Name.Trim();
            string teacherSchool = "Køge Skole";

            bool groupAlreadyExists = _context.Groups
                .Any(g => g.Name.ToLower() == name.ToLower()
                       && g.School == teacherSchool);

            if (groupAlreadyExists)
                return BadRequest(new { message = "Der findes allerede en gruppe med dette navn på skolen" });

            var group = new Group
            {
                Name = name,
                School = teacherSchool,
                IsLocked = false
            };

            var created = _repo.Add(group);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // Kun lærer/user må redigere grupper
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public ActionResult<Group> Update(int id, [FromBody] UpdateGroupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Gruppens navn skal udfyldes" });

            string name = request.Name.Trim();
            string teacherSchool = "Køge Skole";

            bool groupAlreadyExists = _context.Groups
                .Any(g => g.Id != id
                       && g.Name.ToLower() == name.ToLower()
                       && g.School == teacherSchool);

            if (groupAlreadyExists)
                return BadRequest(new { message = "Der findes allerede en gruppe med dette navn på skolen" });

            var existing = _context.Groups.Find(id);
            if (existing == null)
                return NotFound(new { message = "Gruppen blev ikke fundet" });

            existing.Name = name;
            existing.School = teacherSchool;
            _context.SaveChanges();

            return Ok(existing);
        }

        // Kun lærer/user må slette grupper
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public ActionResult<Group> Delete(int id)
        {
            var group = _context.Groups.FirstOrDefault(g => g.Id == id);

            if (group == null)
                return NotFound(new { message = "Gruppen blev ikke fundet" });

            var sessions = _context.Sessions
                .Where(s => s.GroupId == id)
                .ToList();

            foreach (var session in sessions)
            {
                var measurements = _context.Measurements
                    .Where(m => m.SessionId == session.Id)
                    .ToList();

                _context.Measurements.RemoveRange(measurements);
            }

            _context.Sessions.RemoveRange(sessions);
            _context.Groups.Remove(group);
            _context.SaveChanges();

            return Ok(group);
        }

        // Lærer kan se grupper med sessions og målinger
        [Authorize(Roles = "admin")]
        [HttpGet("overview")]
        public IActionResult GetGroupsOverview()
        {
            var overview = _context.Groups
                .Select(g => new
                {
                    g.Id,
                    g.Name,
                    g.School,
                    g.IsLocked,
                    Sessions = _context.Sessions
                        .Where(s => s.GroupId == g.Id)
                        .Select(s => new
                        {
                            s.Id,
                            s.CarType,
                            s.RoadType,
                            s.SpeedLimit,
                            s.Status,
                            s.CreatedAt,
                            s.EndedAt,
                            Measurements = _context.Measurements
                                .Where(m => m.SessionId == s.Id)
                                .OrderByDescending(m => m.CreatedAt)
                                .Select(m => new
                                {
                                    m.Id,
                                    m.MeasuredSpeed,
                                    m.SimulatedSpeed,
                                    m.Time,
                                    m.Distance,
                                    m.Co2,
                                    m.Co2Saved,
                                    m.CreatedAt
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToList();

            return Ok(overview);
        }
    }
}