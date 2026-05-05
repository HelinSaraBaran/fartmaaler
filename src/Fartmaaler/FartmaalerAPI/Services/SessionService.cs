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

        // Afslutter en session og frigiver gruppen igen
        public Session? EndSession(int id)
        {
            // Finder session ud fra id
            var session = _context.Sessions.FirstOrDefault(s => s.Id == id);

            if (session == null)
                return null;

            // Hvis session allerede er afsluttet returneres den bare
            if (session.Status == "Ended")
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

            // Filtrerer fra en bestemt dato hvis startDate er valgt
            if (startDate.HasValue)
            {
                sessions = sessions
                    .Where(s => s.CreatedAt >= startDate.Value)
                    .ToList();
            }

            // Filtrerer til en bestemt dato hvis endDate er valgt
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

                    // Gennemsnit CO2
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