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

        public SessionsController(
            IRepository<Session> repo,
            AppDbContext context,
            SessionService sessionService)
        {
            _repo = repo;
            _context = context;
            _sessionService = sessionService;
        }


        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult GetAll()
        {
            var sessions = _repo.GetAll().ToList();

            if (!sessions.Any())
            {
                return Ok(new
                {
                    message = "Ingen sessions fundet",

                });
            }

            return Ok(new
            {
                totalSessions = sessions.Count,
                sessions = sessions
            });
        }

        [Authorize(Roles = "admin")]
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

        [HttpPost]
        public ActionResult<Session> Add([FromBody] StartSessionRequest request)
        {
            if (request.GroupId <= 0)
            {
                return BadRequest(new { message = "GroupId er påkrævet" });
            }

            if (string.IsNullOrWhiteSpace(request.CarType))
            {
                return BadRequest(new { message = "Biltype er påkrævet" });
            }

            if (string.IsNullOrWhiteSpace(request.RoadType))
            {
                return BadRequest(new { message = "Vejtype er påkrævet" });
            }

            string roadType = request.RoadType.Trim().ToLower();

            if (roadType != "byzone 50" &&
                roadType != "landevej 80" &&
                roadType != "motorvej 130")
            {
                return BadRequest(new { message = "Vejtypen er ikke gyldig" });
            }

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

        
        [HttpPut("{id}/end")]
        public ActionResult<object> EndSession(int id)
        {
            Session? endedSession = _sessionService.EndSession(id);

            if (endedSession == null)
            {
                return NotFound(new { message = "Session blev ikke fundet" });
            }

            return Ok(new
            {
                message = "Session blev afsluttet",
                session = endedSession
            });
        }

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

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            Session? session = _context.Sessions
                .FirstOrDefault(session => session.Id == id);

            if (session == null)
            {
                return NotFound(new { message = "Session blev ikke fundet" });
            }

            List<Measurement> measurements = _context.Measurements
                .Where(measurement => measurement.SessionId == id)
                .ToList();

            _context.Measurements.RemoveRange(measurements);

            Group? group = _context.Groups
                .FirstOrDefault(group => group.Id == session.GroupId);

            if (group != null)
            {
                group.IsLocked = false;
            }

            _context.Sessions.Remove(session);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Session og tilhørende målinger blev slettet"
            });
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("all")]
        public IActionResult DeleteAll()
        {
            try
            {
                _context.Measurements.RemoveRange(_context.Measurements);
                _context.Sessions.RemoveRange(_context.Sessions);

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


        [Authorize(Roles = "admin")]
        [HttpGet("admin")]
        public IActionResult GetAdminSessions(
    string? sortBy = "date",
    string? sortDirection = "desc",
    string? carType = null,
    string? roadType = null,
    string? status = null,
    DateTime? startDate = null,
    DateTime? endDate = null,
    string? groupName = null)
        {
            var sessions = _context.Sessions
                .Select(s => new
                {
                    SessionId = s.Id,
                    GroupName = s.Group != null ? s.Group.Name : "Ukendt gruppe",
                    Date = s.CreatedAt,
                    CarType = s.CarType,
                    RoadType = s.RoadType,
                    Status = s.Status,

                    MeasurementCount = _context.Measurements
                        .Count(m => m.SessionId == s.Id),

                    AverageSpeed = _context.Measurements
                        .Where(m => m.SessionId == s.Id)
                        .Average(m => (double?)m.SimulatedSpeed) ?? 0
                })
                .ToList();


            if (!string.IsNullOrWhiteSpace(carType))
            {
                sessions = sessions
                    .Where(s => s.CarType.ToLower() == carType.ToLower())
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(roadType))
            {
                sessions = sessions
                    .Where(s => s.RoadType.ToLower() == roadType.ToLower())
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                sessions = sessions
                    .Where(s => s.Status.ToLower() == status.ToLower())
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(groupName))
            {
                sessions = sessions
                    .Where(s => s.GroupName.ToLower().Contains(groupName.ToLower()))
                    .ToList();
            }

            if (startDate.HasValue)
            {
                sessions = sessions
                    .Where(s => s.Date >= startDate.Value)
                    .ToList();
            }

            if (endDate.HasValue)
            {
                sessions = sessions
                    .Where(s => s.Date <= endDate.Value)
                    .ToList();
            }


            bool descending = sortDirection?.ToLower() == "desc";

            sessions = sortBy?.ToLower() switch
            {
                "groupname" => descending
                    ? sessions.OrderByDescending(s => s.GroupName).ToList()
                    : sessions.OrderBy(s => s.GroupName).ToList(),

                "cartype" => descending
                    ? sessions.OrderByDescending(s => s.CarType).ToList()
                    : sessions.OrderBy(s => s.CarType).ToList(),

                "roadtype" => descending
                    ? sessions.OrderByDescending(s => s.RoadType).ToList()
                    : sessions.OrderBy(s => s.RoadType).ToList(),

                "speed" or "averagespeed" => descending
                    ? sessions.OrderByDescending(s => s.AverageSpeed).ToList()
                    : sessions.OrderBy(s => s.AverageSpeed).ToList(),

                "measurements" or "measurementcount" => descending
                    ? sessions.OrderByDescending(s => s.MeasurementCount).ToList()
                    : sessions.OrderBy(s => s.MeasurementCount).ToList(),

                _ => descending
                    ? sessions.OrderByDescending(s => s.Date).ToList()
                    : sessions.OrderBy(s => s.Date).ToList()
            };

            double classAverageSpeed = sessions.Any()
                ? Math.Round(sessions.Average(s => s.AverageSpeed), 2)
                : 0;

            if (!sessions.Any())
            {
                return Ok(new

                {
                    message = "Ingen sessions fundet",
                 

                });
            }


            return Ok(new
            {
               
                ClassAverageSpeed = classAverageSpeed,
                TotalSessions = sessions.Count,
                Sessions = sessions.Select(s => new
                {
                    s.SessionId,
                    s.GroupName,
                    s.Date,
                    s.CarType,
                    s.RoadType,
                    s.Status,
                    s.MeasurementCount,
                    AverageSpeed = Math.Round(s.AverageSpeed, 2)
                })
            });
        }
    }
}