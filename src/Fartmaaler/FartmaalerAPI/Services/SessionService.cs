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
        // Bruges når en session oprettes
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
        // Legetøjsbilens hastighed ganges med denne for at simulere realistiske tal
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

        // Afslutter en session og frigiver gruppen igen
        public Session? EndSession(int id)
        {
            // Finder session ud fra id
            var session = _context.Sessions.FirstOrDefault(s => s.Id == id);

            // Hvis session ikke findes returneres null
            if (session == null)
                return null;

            // Hvis session allerede er afsluttet returneres den bare
            if (session.Status?.ToLower() == "ended")
                return session;

            // Sætter session som afsluttet
            session.Status = "Ended";
            session.EndedAt = DateTime.Now;

            // Finder gruppen der hører til sessionen
            var group = _context.Groups.FirstOrDefault(g => g.Id == session.GroupId);

            // Låser gruppen op igen
            if (group != null)
            {
                group.IsLocked = false;
            }

            // Gemmer ændringer i databasen
            _context.SaveChanges();

            return session;
        }

        // Henter historik for en bestemt gruppe
        public object? GetHistoryByGroup(
            int groupId,
            string? carType,
            string? roadType,
            DateTime? startDate,
            DateTime? endDate)
        {
            // Finder gruppen
            var group = _context.Groups.FirstOrDefault(g => g.Id == groupId);

            // Hvis gruppen ikke findes returneres null
            if (group == null)
                return null;

            // Henter alle sessions for gruppen
            var sessions = _context.Sessions
                .Where(s => s.GroupId == groupId)
                .ToList();

            // Filtrerer på biltype hvis valgt
            if (!string.IsNullOrWhiteSpace(carType))
            {
                sessions = sessions
                    .Where(s => s.CarType.ToLower() == carType.ToLower())
                    .ToList();
            }

            // Filtrerer på vejtype hvis valgt
            if (!string.IsNullOrWhiteSpace(roadType))
            {
                sessions = sessions
                    .Where(s => s.RoadType.ToLower() == roadType.ToLower())
                    .ToList();
            }

            // Filtrerer fra bestemt dato
            if (startDate.HasValue)
            {
                sessions = sessions
                    .Where(s => s.CreatedAt >= startDate.Value)
                    .ToList();
            }

            // Filtrerer til bestemt dato
            if (endDate.HasValue)
            {
                sessions = sessions
                    .Where(s => s.CreatedAt <= endDate.Value)
                    .ToList();
            }

            // Laver historik med beregninger
            var history = sessions
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new
                {
                    s.Id,
                    GroupName = group.Name,
                    s.CarType,
                    s.RoadType,
                    s.SpeedLimit,
                    s.Status,
                    s.CreatedAt,
                    s.EndedAt,

                    // Antal målinger i sessionen
                    MeasurementCount = _context.Measurements
                        .Count(m => m.SessionId == s.Id),

                    // Gennemsnitshastighed
                    AverageSpeed = _context.Measurements
                        .Where(m => m.SessionId == s.Id)
                        .Average(m => (double?)m.SimulatedSpeed) ?? 0,

                    // Gennemsnitlig CO2
                    AverageCo2 = _context.Measurements
                        .Where(m => m.SessionId == s.Id)
                        .Average(m => (double?)m.Co2) ?? 0,

                    // Samlet CO2 besparelse
                    TotalCo2Saved = _context.Measurements
                        .Where(m => m.SessionId == s.Id)
                        .Sum(m => (double?)m.Co2Saved) ?? 0
                })
                .ToList();

            return history;
        }
    }
}