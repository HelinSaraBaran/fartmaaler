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
            List<Session> sessions =
                _repo.GetAll().ToList();

            if (!sessions.Any())
            {
                return Ok(new
                {
                    message = "Ingen sessions fundet",
                    totalSessions = 0,
                    sessions = new List<Session>()
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
            Session? session =
                _repo.GetById(id);

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

            string roadType =
                request.RoadType.Trim().ToLower();

            if (roadType != "byzone 50" &&
                roadType != "landevej 80" &&
                roadType != "motorvej 110")
            {
                return BadRequest(new { message = "Vejtypen er ikke gyldig" });
            }

            Group? group =
                _context.Groups.FirstOrDefault(groupItem => groupItem.Id == request.GroupId);

            if (group == null)
            {
                return NotFound(new { message = "Gruppen blev ikke fundet" });
            }

            if (group.IsLocked)
            {
                return BadRequest(new { message = "Gruppen er allerede i gang med en session" });
            }

            Session? createdSession =
                _sessionService.StartSession(
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
            Session? endedSession =
                _sessionService.EndSession(id);

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
            object? history =
                _sessionService.GetHistoryByGroup(
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
            Session? updated =
                _repo.Update(id, session);

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
            Session? session =
                _context.Sessions.FirstOrDefault(sessionItem => sessionItem.Id == id);

            if (session == null)
            {
                return NotFound(new { message = "Session blev ikke fundet" });
            }

            List<Measurement> measurements =
                _context.Measurements
                    .Where(measurement => measurement.SessionId == id)
                    .ToList();

            _context.Measurements.RemoveRange(measurements);

            Group? group =
                _context.Groups.FirstOrDefault(groupItem => groupItem.Id == session.GroupId);

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

                List<Group> groups =
                    _context.Groups
                        .Where(group => group.IsLocked)
                        .ToList();

                foreach (Group group in groups)
                {
                    group.IsLocked = false;
                }

                _context.SaveChanges();

                return Ok(new { message = "Al historik er slettet" });
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


        [Authorize(Roles = "admin")]
        [HttpGet("class-summary")]
        public IActionResult GetClassSummary()
        {
            try
            {
                List<Measurement> measurements =
                    _context.Measurements.ToList();

                if (!measurements.Any())
                {
                    return Ok(new
                    {
                        message = "Ingen data endnu",
                        count = 0,
                        totalMeasurements = 0,
                        averageSpeed = 0,
                        averageCo2 = 0,
                        averageDeviation = 0,
                        averageScore = 0,
                        totalSessions = _context.Sessions.Count()
                    });
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
            catch (Exception exception)
            {
                return StatusCode(500, new
                {
                    message = "Der opstod en fejl",
                    error = exception.Message
                });
            }
        }

        [HttpGet("active/latest")]
        public IActionResult GetLatestActiveSession()
        {
            var session = _context.Sessions
                .Where(s => s.Status == "Active")
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefault();

            if (session == null)
                return NotFound(new { message = "Ingen aktiv session fundet" });

            return Ok(session);
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
            List<AdminSessionResponse> sessions =
                _context.Sessions
                    .Select(session => new AdminSessionResponse
                    {
                        SessionId = session.Id,
                        GroupName = session.Group != null ? session.Group.Name : "Ukendt gruppe",
                        Date = session.CreatedAt,
                        CarType = session.CarType,
                        RoadType = session.RoadType,
                        Status = session.Status,
                        MeasurementCount = _context.Measurements.Count(measurement => measurement.SessionId == session.Id),
                        AverageSpeed = _context.Measurements
                            .Where(measurement => measurement.SessionId == session.Id)
                            .Average(measurement => (double?)measurement.SimulatedSpeed) ?? 0
                    })
                    .ToList();

            if (!string.IsNullOrWhiteSpace(carType))
            {
                sessions =
                    sessions
                        .Where(session => session.CarType.ToLower() == carType.ToLower())
                        .ToList();
            }

            if (!string.IsNullOrWhiteSpace(roadType))
            {
                sessions =
                    sessions
                        .Where(session => session.RoadType.ToLower() == roadType.ToLower())
                        .ToList();
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                sessions =
                    sessions
                        .Where(session => session.Status.ToLower() == status.ToLower())
                        .ToList();
            }

            if (!string.IsNullOrWhiteSpace(groupName))
            {
                sessions =
                    sessions
                        .Where(session => session.GroupName.ToLower().Contains(groupName.ToLower()))
                        .ToList();
            }

            if (startDate.HasValue)
            {
                sessions =
                    sessions
                        .Where(session => session.Date >= startDate.Value)
                        .ToList();
            }

            if (endDate.HasValue)
            {
                sessions =
                    sessions
                        .Where(session => session.Date <= endDate.Value)
                        .ToList();
            }

            bool descending =
                sortDirection != null && sortDirection.ToLower() == "desc";

            string selectedSort =
                "date";

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                selectedSort =
                    sortBy.ToLower();
            }

            if (selectedSort == "groupname")
            {
                if (descending)
                {
                    sessions =
                        sessions.OrderByDescending(session => session.GroupName).ToList();
                }
                else
                {
                    sessions =
                        sessions.OrderBy(session => session.GroupName).ToList();
                }
            }
            else if (selectedSort == "cartype")
            {
                if (descending)
                {
                    sessions =
                        sessions.OrderByDescending(session => session.CarType).ToList();
                }
                else
                {
                    sessions =
                        sessions.OrderBy(session => session.CarType).ToList();
                }
            }
            else if (selectedSort == "roadtype")
            {
                if (descending)
                {
                    sessions =
                        sessions.OrderByDescending(session => session.RoadType).ToList();
                }
                else
                {
                    sessions =
                        sessions.OrderBy(session => session.RoadType).ToList();
                }
            }
            else if (selectedSort == "speed" || selectedSort == "averagespeed")
            {
                if (descending)
                {
                    sessions =
                        sessions.OrderByDescending(session => session.AverageSpeed).ToList();
                }
                else
                {
                    sessions =
                        sessions.OrderBy(session => session.AverageSpeed).ToList();
                }
            }
            else if (selectedSort == "measurements" || selectedSort == "measurementcount")
            {
                if (descending)
                {
                    sessions =
                        sessions.OrderByDescending(session => session.MeasurementCount).ToList();
                }
                else
                {
                    sessions =
                        sessions.OrderBy(session => session.MeasurementCount).ToList();
                }
            }
            else
            {
                if (descending)
                {
                    sessions =
                        sessions.OrderByDescending(session => session.Date).ToList();
                }
                else
                {
                    sessions =
                        sessions.OrderBy(session => session.Date).ToList();
                }
            }

            double classAverageSpeed =
                0;

            if (sessions.Any())
            {
                classAverageSpeed =
                    Math.Round(sessions.Average(session => session.AverageSpeed), 2);
            }

            return Ok(new
            {
                Message = sessions.Any() ? "Sessions fundet" : "Ingen sessions fundet",
                ClassAverageSpeed = classAverageSpeed,
                TotalSessions = sessions.Count,
                Sessions = sessions.Select(session => new
                {
                    session.SessionId,
                    session.GroupName,
                    session.Date,
                    session.CarType,
                    session.RoadType,
                    session.Status,
                    session.MeasurementCount,
                    AverageSpeed = Math.Round(session.AverageSpeed, 2)
                }).ToList()
            });
        }
    }

    public class AdminSessionResponse
    {
        public int SessionId { get; set; }

        public string GroupName { get; set; } = "";

        public DateTime Date { get; set; }

        public string CarType { get; set; } = "";

        public string RoadType { get; set; } = "";

        public string Status { get; set; } = "";

        public int MeasurementCount { get; set; }

        public double AverageSpeed { get; set; }
    }
}