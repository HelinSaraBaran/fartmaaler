using FartmaalerAPI.Data;
using FartmaalerAPI.Models;

namespace FartmaalerAPI.Services
{
    // Denne service håndterer ekstra logik for sessions
    // Controlleren bruger denne klasse så controlleren ikke får for meget ansvar
    public class SessionService
    {
        // DbContext bruges til at hente og gemme data i databasen
        private readonly AppDbContext _context;

        // Constructor modtager DbContext via dependency injection
        public SessionService(AppDbContext context)
        {
            _context = context;
        }

        // Returnerer hastighedsgrænsen baseret på vejtype
        public int GetSpeedLimit(string roadType)
        {
            return roadType?.ToLower() switch
            {
                "byzone 50" => 50,
                "landevej 80" => 80,
                "motorvej 130" => 130,
                _ => 50
            };
        }

        // Returnerer skaleringsfaktoren baseret på vejtype
        public double GetScalingFactor(string roadType)
        {
            return roadType?.ToLower() switch
            {
                "byzone 50" => 10,
                "landevej 80" => 15,
                "motorvej 130" => 20,
                _ => 10
            };
        }

        // Starter en ny session og låser gruppen
        public Session? StartSession(int groupId, string carType, string roadType)
        {
            Group? group = _context.Groups.FirstOrDefault(group => group.Id == groupId);

            if (group == null)
            {
                return null;
            }

            if (group.IsLocked == true)
            {
                return null;
            }

            Session session = new Session
            {
                GroupId = groupId,
                CarType = carType,
                RoadType = roadType,
                SpeedLimit = GetSpeedLimit(roadType),
                ScalingFactor = GetScalingFactor(roadType),
                Status = "Active",
                CreatedAt = DateTime.Now,
                EndedAt = null
            };

            group.IsLocked = true;

            _context.Sessions.Add(session);
            _context.SaveChanges();

            return session;
        }

        // Afslutter en session og frigiver gruppen igen
        public Session? EndSession(int id)
        {
            Session? session = _context.Sessions.FirstOrDefault(session => session.Id == id);

            if (session == null)
            {
                return null;
            }

            if (session.Status?.ToLower() == "ended")
            {
                return session;
            }

            session.Status = "Ended";
            session.EndedAt = DateTime.Now;

            Group? group = _context.Groups.FirstOrDefault(group => group.Id == session.GroupId);

            if (group != null)
            {
                group.IsLocked = false;
            }

            _context.SaveChanges();

            return session;
        }

        // Henter historik for en bestemt gruppe med filter og sortering
        public object? GetHistoryByGroup(
            int groupId,
            string? carType,
            string? roadType,
            DateTime? startDate,
            DateTime? endDate,
            string? sortBy,
            string? sortDirection)
        {
            Group? group = _context.Groups.FirstOrDefault(group => group.Id == groupId);

            if (group == null)
            {
                return null;
            }

            List<Session> sessions = _context.Sessions
                .Where(session => session.GroupId == groupId)
                .ToList();

            // Filtrerer på biltype
            if (!string.IsNullOrWhiteSpace(carType))
            {
                sessions = sessions
                    .Where(session => session.CarType.ToLower() == carType.ToLower())
                    .ToList();
            }

            // Filtrerer på vejtype
            if (!string.IsNullOrWhiteSpace(roadType))
            {
                sessions = sessions
                    .Where(session => session.RoadType.ToLower() == roadType.ToLower())
                    .ToList();
            }

            // Filtrerer fra dato
            if (startDate.HasValue)
            {
                sessions = sessions
                    .Where(session => session.CreatedAt >= startDate.Value)
                    .ToList();
            }

            // Filtrerer til dato
            if (endDate.HasValue)
            {
                sessions = sessions
                    .Where(session => session.CreatedAt <= endDate.Value)
                    .ToList();
            }

            var history = sessions
                .Select(session => new
                {
                    session.Id,
                    GroupName = group.Name,
                    session.CarType,
                    session.RoadType,
                    session.SpeedLimit,
                    session.Status,
                    session.CreatedAt,
                    session.EndedAt,

                    // Antal målinger i sessionen
                    MeasurementCount = _context.Measurements
                        .Count(measurement => measurement.SessionId == session.Id),

                    // Gennemsnitshastighed
                    AverageSpeed = _context.Measurements
                        .Where(measurement => measurement.SessionId == session.Id)
                        .Average(measurement => (double?)measurement.SimulatedSpeed) ?? 0,

                    // Gennemsnitlig CO2
                    AverageCo2 = _context.Measurements
                        .Where(measurement => measurement.SessionId == session.Id)
                        .Average(measurement => (double?)measurement.Co2) ?? 0,

                    // Gennemsnitlig tid
                    AverageTime = _context.Measurements
                        .Where(measurement => measurement.SessionId == session.Id)
                        .Average(measurement => (double?)measurement.Time) ?? 0,

                    // Samlet CO2 besparelse
                    TotalCo2Saved = _context.Measurements
                        .Where(measurement => measurement.SessionId == session.Id)
                        .Sum(measurement => (double?)measurement.Co2Saved) ?? 0
                })
                .ToList();

            bool descending = sortDirection?.ToLower() == "desc";

            // Sorterer historikken
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy.ToLower() == "co2")
                {
                    history = descending
                        ? history.OrderByDescending(item => item.AverageCo2).ToList()
                        : history.OrderBy(item => item.AverageCo2).ToList();
                }
                else if (sortBy.ToLower() == "speed")
                {
                    history = descending
                        ? history.OrderByDescending(item => item.AverageSpeed).ToList()
                        : history.OrderBy(item => item.AverageSpeed).ToList();
                }
                else if (sortBy.ToLower() == "time")
                {
                    history = descending
                        ? history.OrderByDescending(item => item.AverageTime).ToList()
                        : history.OrderBy(item => item.AverageTime).ToList();
                }
                else
                {
                    history = history
                        .OrderByDescending(item => item.CreatedAt)
                        .ToList();
                }
            }
            else
            {
                history = history
                    .OrderByDescending(item => item.CreatedAt)
                    .ToList();
            }

            return history;
        }
    }
}