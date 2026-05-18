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
                "motorvej 110" => 110,
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
                "motorvej 110" => 20,
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

            // Filtrering på biltype
            if (!string.IsNullOrWhiteSpace(carType))
            {
                sessions = sessions
                    .Where(session =>
                        session.CarType.ToLower() ==
                        carType.ToLower())
                    .ToList();
            }

            // Filtrering på vejtype
            if (!string.IsNullOrWhiteSpace(roadType))
            {
                sessions = sessions
                    .Where(session =>
                        session.RoadType.ToLower() ==
                        roadType.ToLower())
                    .ToList();
            }

            // Filtrering fra dato
            if (startDate.HasValue)
            {
                sessions = sessions
                    .Where(session =>
                        session.CreatedAt >= startDate.Value)
                    .ToList();
            }

            // Filtrering til dato
            if (endDate.HasValue)
            {
                sessions = sessions
                    .Where(session =>
                        session.CreatedAt <= endDate.Value)
                    .ToList();
            }

            var history = sessions
                .Select(session =>
                {
                    List<Measurement> measurements =
                        _context.Measurements
                            .Where(measurement =>
                                measurement.SessionId == session.Id)
                            .ToList();

                    double averageSpeed = 0;
                    double averageCo2 = 0;
                    double averageTime = 0;
                    double totalCo2Saved = 0;
                    double score = 0;

                    if (measurements.Count > 0)
                    {
                        averageSpeed =
                            measurements.Average(measurement =>
                                measurement.SimulatedSpeed);

                        averageCo2 =
                            measurements.Average(measurement =>
                                measurement.Co2);

                        averageTime =
                            measurements.Average(measurement =>
                                measurement.Time);

                        totalCo2Saved =
                            measurements.Sum(measurement =>
                                measurement.Co2Saved);

                        score =
                            measurements.Average(measurement =>
                                Math.Abs(
                                    measurement.SimulatedSpeed -
                                    measurement.SpeedLimit
                                ) + measurement.Co2);
                    }

                    return new
                    {
                        session.Id,

                        GroupName = group.Name,

                        session.CarType,
                        session.RoadType,
                        session.SpeedLimit,
                        session.Status,

                        Date = session.CreatedAt,

                        session.CreatedAt,
                        session.EndedAt,

                        MeasurementCount = measurements.Count,

                        AverageSpeed =
                            Math.Round(averageSpeed, 2),

                        AverageCo2 =
                            Math.Round(averageCo2, 2),

                        AverageTime =
                            Math.Round(averageTime, 2),

                        TotalCo2Saved =
                            Math.Round(totalCo2Saved, 2),

                        Co2 =
                            Math.Round(averageCo2, 2),

                        Score =
                            Math.Round(score, 2),

                        Measurements =
                            measurements.Select(measurement => new
                            {
                                measurement.Id,
                                measurement.SessionId,

                                measurement.Time,
                                measurement.Distance,

                                Speed =
                                    Math.Round(
                                        measurement.SimulatedSpeed,
                                        2
                                    ),

                                measurement.SimulatedSpeed,
                                measurement.SpeedLimit,

                                measurement.Co2,
                                measurement.Co2Saved,

                                Score =
                                    Math.Round(
                                        Math.Abs(
                                            measurement.SimulatedSpeed -
                                            measurement.SpeedLimit
                                        ) + measurement.Co2,
                                        2
                                    )
                            })
                            .ToList()
                    };
                })
                .ToList();

            bool descending =
                sortDirection?.ToLower() == "desc";

            // Sortering
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (
                    sortBy.ToLower() == "co2" ||
                    sortBy.ToLower() == "bestco2"
                )
                {
                    history = descending
                        ? history.OrderByDescending(item =>
                            item.AverageCo2).ToList()
                        : history.OrderBy(item =>
                            item.AverageCo2).ToList();
                }

                else if (
                    sortBy.ToLower() == "speed" ||
                    sortBy.ToLower() == "speedhigh"
                )
                {
                    history = descending
                        ? history.OrderByDescending(item =>
                            item.AverageSpeed).ToList()
                        : history.OrderBy(item =>
                            item.AverageSpeed).ToList();
                }

                else if (sortBy.ToLower() == "speedlow")
                {
                    history = history
                        .OrderBy(item =>
                            item.AverageSpeed)
                        .ToList();
                }

                else if (
                    sortBy.ToLower() == "time" ||
                    sortBy.ToLower() == "timelow"
                )
                {
                    history = history
                        .OrderBy(item =>
                            item.AverageTime)
                        .ToList();
                }

                else if (sortBy.ToLower() == "score")
                {
                    history = history
                        .OrderBy(item =>
                            item.Score)
                        .ToList();
                }

                else
                {
                    history = history
                        .OrderByDescending(item =>
                            item.CreatedAt)
                        .ToList();
                }
            }
            else
            {
                history = history
                    .OrderByDescending(item =>
                        item.CreatedAt)
                    .ToList();
            }

            return history;
        }
    }
}