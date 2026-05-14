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
    public class MeasurementsController : ControllerBase
    {
        private readonly IRepository<Measurement> _repo;
        private readonly AppDbContext _context;
        private readonly MeasurementService _measurementService;

        public MeasurementsController(
            IRepository<Measurement> repo,
            AppDbContext context,
            MeasurementService measurementService)
        {
            _repo = repo;
            _context = context;
            _measurementService = measurementService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var measurements = _repo.GetAll().ToList();

            if (!measurements.Any())
            {
                return Ok(new
                {
                    message = "Ingen målinger fundet",
                  
                });
            }

            return Ok(measurements);
            
              
        }

        [HttpGet("{id}")]
        public ActionResult<Measurement> GetById(int id)
        {
            var measurement = _repo.GetById(id);

            if (measurement == null)
                return NotFound(new { message = "Måling blev ikke fundet" });

            return Ok(measurement);
        }

        // Opretter en måling ud fra sessionId og tid mellem sensorerne
        [HttpPost]
        public ActionResult<Measurement> Add([FromBody] CreateMeasurementRequest request)
        {
            if (request.SessionId <= 0)
                return BadRequest(new { message = "SessionId er påkrævet" });

            if (request.Time <= 0)
                return BadRequest(new { message = "Tid skal være større end 0" });

            var session = _context.Sessions.FirstOrDefault(s => s.Id == request.SessionId);

            if (session == null)
                return NotFound(new { message = "Session blev ikke fundet" });

            if (session.Status?.ToLower() == "ended")
                return BadRequest(new { message = "Der kan ikke tilføjes målinger til en afsluttet session" });

            var created = _measurementService.CreateMeasurement(
                request.SessionId,
                request.Time
            );

            if (created == null)
                return BadRequest(new { message = "Målingen kunne ikke oprettes" });

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize (Roles = "admin")]
        [HttpDelete("{id}")]
        public ActionResult<Measurement> Delete(int id)
        {
            var deleted = _repo.Delete(id);

            if (deleted == null)
                return NotFound(new { message = "Måling blev ikke fundet" });

            return Ok(deleted);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Measurement measurement)
        {
            return BadRequest(new { message = "Målinger kan ikke opdateres" });
        }

        [Authorize(Roles = "admin")]
        [HttpGet("live-overview")]
        public IActionResult GetLiveOverview()
        {
            var result = _context.Groups
                .Select(g => new
                {
                    GroupId = g.Id,
                    GroupName = g.Name,
                    School = g.School,
                    IsLocked = g.IsLocked,

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
                            m.SpeedLimit,
                            m.Status,
                            m.Co2,
                            m.Co2Saved,
                            m.CreatedAt
                        })
                        .FirstOrDefault(),

                    Status = _context.Measurements
                        .Any(m => _context.Sessions
                            .Any(s => s.Id == m.SessionId && s.GroupId == g.Id))
                            ? "Måling registreret"
                            : "Venter på måling"
                })
                .ToList();

            return Ok(result);
        }

        [HttpGet("leaderboard")]
        public IActionResult GetLeaderboard()
        {
            var result = _context.Groups
                .Select(g => new
                {
                    GroupId = g.Id,
                    GroupName = g.Name,
                    School = g.School,

                    AvgDeviation = _context.Measurements
                        .Where(m => _context.Sessions
                            .Any(s => s.Id == m.SessionId && s.GroupId == g.Id))
                        .Average(m => (double?)Math.Abs(m.SimulatedSpeed - m.SpeedLimit)) ?? 0,

                    AvgCo2 = _context.Measurements
                        .Where(m => _context.Sessions
                            .Any(s => s.Id == m.SessionId && s.GroupId == g.Id))
                        .Average(m => (double?)m.Co2) ?? 0,

                    Count = _context.Measurements
                        .Count(m => _context.Sessions
                            .Any(s => s.Id == m.SessionId && s.GroupId == g.Id))
                })
                .Where(x => x.Count > 0)
                .ToList()
                .Select(x => new
                {
                    x.GroupId,
                    x.GroupName,
                    x.School,
                    Score = Math.Round(x.AvgDeviation + x.AvgCo2, 2),
                    x.AvgDeviation,
                    x.AvgCo2,
                    x.Count
                })
                .OrderBy(x => x.Score)
                .ToList();

            if (!result.Any())
                return NotFound(new { message = "Ingen data til leaderboard" });

            return Ok(result);
        }

        [HttpGet("session/{sessionId}")]
        public IActionResult GetMeasurementsBySession(int sessionId)
        {
            var session = _context.Sessions.FirstOrDefault(s => s.Id == sessionId);

            if (session == null)
                return NotFound(new { message = "Session blev ikke fundet" });

            var measurements = _context.Measurements
                .Where(m => m.SessionId == sessionId)
                .OrderByDescending(m => m.CreatedAt)
                .ToList();

            return Ok(measurements);
        }

        [HttpGet("session/{sessionId}/summary")]
        public IActionResult GetSessionSummary(int sessionId)
        {
            var session = _context.Sessions.FirstOrDefault(s => s.Id == sessionId);

            if (session == null)
                return NotFound(new { message = "Session blev ikke fundet" });

            var measurements = _context.Measurements
                .Where(m => m.SessionId == sessionId)
                .ToList();

            if (!measurements.Any())
                return Ok(new
                {
                    Count = 0,
                    AverageSpeed = 0,
                    AverageCo2 = 0,
                    TotalCo2Saved = 0
                });

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