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

        // Henter alle grupper til dropdown og gruppe oversigt.
        [HttpGet]
        public ActionResult<IEnumerable<Group>> GetAll()
        {
            IEnumerable<Group> groups =
                _repo.GetAll();

            return Ok(groups);
        }

        // Henter en gruppe ud fra id.
        [Authorize(Roles = "admin")]
        [HttpGet("{id}")]
        public ActionResult<Group> GetById(int id)
        {
            Group? group =
                _repo.GetById(id);

            if (group == null)
            {
                return NotFound(new { message = "Gruppen blev ikke fundet" });
            }

            return Ok(group);
        }

        // Opretter en ny gruppe.
        [Authorize(Roles = "admin")]
        [HttpPost]
        public ActionResult<Group> Add([FromBody] CreateGroupRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Data mangler" });
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "Gruppens navn skal udfyldes" });
            }

            string name =
                request.Name.Trim();

            string teacherSchool =
                "Roskilde Skole";

            bool groupAlreadyExists =
                _context.Groups.Any(group =>
                    group.Name.ToLower() == name.ToLower()
                    &&
                    group.School == teacherSchool
                );

            if (groupAlreadyExists)
            {
                return BadRequest(new { message = "Der findes allerede en gruppe med dette navn på skolen" });
            }

            Group group =
                new Group
                {
                    Name = name,
                    School = teacherSchool,
                    IsLocked = false
                };

            Group created =
                _repo.Add(group);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                created
            );
        }

        // Opdaterer en eksisterende gruppe.
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public ActionResult<Group> Update(int id, [FromBody] UpdateGroupRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Data mangler" });
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "Gruppens navn skal udfyldes" });
            }

            string name =
                request.Name.Trim();

            string teacherSchool =
                "Roskilde Skole";

            bool groupAlreadyExists =
                _context.Groups.Any(group =>
                    group.Id != id
                    &&
                    group.Name.ToLower() == name.ToLower()
                    &&
                    group.School == teacherSchool
                );

            if (groupAlreadyExists)
            {
                return BadRequest(new { message = "Der findes allerede en gruppe med dette navn på skolen" });
            }

            Group? existing =
                _context.Groups.Find(id);

            if (existing == null)
            {
                return NotFound(new { message = "Gruppen blev ikke fundet" });
            }

            existing.Name =
                name;

            existing.School =
                teacherSchool;

            existing.IsLocked =
                false;

            _context.SaveChanges();

            return Ok(existing);
        }

        // Sletter en gruppe og dens sessions og målinger.
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            Group? group =
                _context.Groups.FirstOrDefault(groupItem => groupItem.Id == id);

            if (group == null)
            {
                return NotFound(new { message = "Gruppen blev ikke fundet" });
            }

            List<Session> sessions =
                _context.Sessions
                    .Where(session => session.GroupId == id)
                    .ToList();

            foreach (Session session in sessions)
            {
                List<Measurement> measurements =
                    _context.Measurements
                        .Where(measurement => measurement.SessionId == session.Id)
                        .ToList();

                _context.Measurements.RemoveRange(measurements);
            }

            _context.Sessions.RemoveRange(sessions);
            _context.Groups.Remove(group);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Gruppe og tilhørende sessions/målinger blev slettet"
            });
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("all")]
        public IActionResult DeleteAllGroups()
        {
            try
            {
                // Slet alle measurements først
                _context.Measurements.RemoveRange(_context.Measurements);

                // Slet alle sessions
                _context.Sessions.RemoveRange(_context.Sessions);

                // Slet alle grupper
                _context.Groups.RemoveRange(_context.Groups);

                _context.SaveChanges();

                return Ok(new
                {
                    message = "Alle grupper, sessions og målinger blev slettet"
                });
            }
            catch (Exception exception)
            {
                return StatusCode(500, new
                {
                    message = "Der opstod en fejl",
                    error = exception.Message
                });
            }
        }

        // Henter grupper med sessions og målinger.
        [Authorize(Roles = "admin")]
        [HttpGet("overview")]
        public IActionResult GetGroupsOverview()
        {
            List<object> overview =
                _context.Groups
                    .Select(group => new
                    {
                        group.Id,
                        group.Name,
                        group.School,
                        group.IsLocked,
                        Sessions = _context.Sessions
                            .Where(session => session.GroupId == group.Id)
                            .Select(session => new
                            {
                                session.Id,
                                session.CarType,
                                session.RoadType,
                                session.SpeedLimit,
                                session.Status,
                                session.CreatedAt,
                                session.EndedAt,
                                Measurements = _context.Measurements
                                    .Where(measurement => measurement.SessionId == session.Id)
                                    .OrderByDescending(measurement => measurement.CreatedAt)
                                    .Select(measurement => new
                                    {
                                        measurement.Id,
                                        measurement.MeasuredSpeed,
                                        measurement.SimulatedSpeed,
                                        measurement.Time,
                                        measurement.Distance,
                                        measurement.Co2,
                                        measurement.Co2Saved,
                                        measurement.CreatedAt
                                    })
                                    .ToList()
                            })
                            .ToList()
                    })
                    .Cast<object>()
                    .ToList();

            return Ok(overview);
        }
    }
}