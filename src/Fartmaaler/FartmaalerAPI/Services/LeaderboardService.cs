using FartmaalerAPI.Data;
using FartmaalerAPI.DTOs;
using FartmaalerAPI.Models;

namespace FartmaalerAPI.Services
{
    // Denne service håndterer leaderboard logik
    public class LeaderboardService
    {
        private readonly AppDbContext _context;

        private const string OwnSchoolName = "Køge Skole";

        public LeaderboardService(AppDbContext context)
        {
            _context = context;
        }

        // Henter klasse leaderboard for en bestemt vejtype
        public List<LeaderboardEntryResponse> GetClassLeaderboard(string roadType)
        {
            List<Session> endedSessions = _context.Sessions
                .Where(session =>
                    session.Status.ToLower() == "ended" &&
                    session.RoadType.ToLower() == roadType.ToLower())
                .ToList();

            List<LeaderboardEntryResponse> allScores = new List<LeaderboardEntryResponse>();

            foreach (Session session in endedSessions)
            {
                Group? group = _context.Groups
                    .FirstOrDefault(group => group.Id == session.GroupId);

                if (group == null)
                    continue;

                List<Measurement> measurements = _context.Measurements
                    .Where(measurement => measurement.SessionId == session.Id)
                    .ToList();

                if (measurements.Count == 0)
                    continue;

                double averageDeviation = measurements
                    .Average(measurement => Math.Abs(measurement.SimulatedSpeed - session.SpeedLimit));

                double averageCo2 = measurements
                    .Average(measurement => measurement.Co2);

                double score = averageDeviation + averageCo2;

                LeaderboardEntryResponse entry = new LeaderboardEntryResponse
                {
                    GroupId = group.Id,
                    GroupName = group.Name,
                    School = group.School,
                    RoadType = session.RoadType,
                    SessionId = session.Id,
                    Score = Math.Round(score, 2),
                    AverageDeviation = Math.Round(averageDeviation, 2),
                    AverageCo2 = Math.Round(averageCo2, 2),
                    MeasurementCount = measurements.Count
                };

                allScores.Add(entry);
            }

            List<LeaderboardEntryResponse> bestScores = allScores
                .GroupBy(entry => entry.GroupId)
                .Select(group => group.OrderBy(entry => entry.Score).First())
                .OrderBy(entry => entry.Score)
                .ToList();

            return bestScores;
        }

        // Henter skole leaderboard for en bestemt vejtype
        public List<SchoolLeaderboardResponse> GetSchoolLeaderboard(string roadType)
        {
            List<LeaderboardEntryResponse> classLeaderboard = GetClassLeaderboard(roadType);
            List<SchoolLeaderboardResponse> schoolLeaderboard = new List<SchoolLeaderboardResponse>();

            if (classLeaderboard.Count > 0)
            {
                SchoolLeaderboardResponse ownSchool = new SchoolLeaderboardResponse
                {
                    SchoolName = OwnSchoolName,
                    RoadType = roadType,
                    AverageScore = Math.Round(classLeaderboard.Average(entry => entry.Score), 2),
                    AverageCo2 = Math.Round(classLeaderboard.Average(entry => entry.AverageCo2), 2),
                    MeasurementCount = classLeaderboard.Sum(entry => entry.MeasurementCount),
                    IsOwnSchool = true
                };

                schoolLeaderboard.Add(ownSchool);
            }

            List<SchoolLeaderboardMock> mockSchools = _context.SchoolLeaderboardMocks
                .Where(mockSchool => mockSchool.RoadType.ToLower() == roadType.ToLower())
                .ToList();

            foreach (SchoolLeaderboardMock mockSchool in mockSchools)
            {
                SchoolLeaderboardResponse response = new SchoolLeaderboardResponse
                {
                    SchoolName = mockSchool.SchoolName,
                    RoadType = mockSchool.RoadType,
                    AverageScore = mockSchool.AverageScore,
                    AverageCo2 = mockSchool.AverageCo2,
                    MeasurementCount = mockSchool.MeasurementCount,
                    IsOwnSchool = false
                };

                schoolLeaderboard.Add(response);
            }

            return schoolLeaderboard
                .OrderBy(school => school.AverageScore)
                .ToList();
        }

        // Tjekker om leaderboard er slået til i Settings
        public bool IsLeaderboardEnabled()
        {
            Settings? setting = _context.Settings
                .FirstOrDefault(setting => setting.Key.ToLower() == "leaderboard");

            if (setting == null)
                return false;

            return setting.Value;
        }

        // Opdaterer leaderboard setting i Settings
        public Settings UpdateLeaderboardSetting(bool isEnabled)
        {
            Settings? setting = _context.Settings
                .FirstOrDefault(setting => setting.Key.ToLower() == "leaderboard");

            if (setting == null)
            {
                setting = new Settings
                {
                    Key = "Leaderboard",
                    Value = isEnabled
                };

                _context.Settings.Add(setting);
            }
            else
            {
                setting.Value = isEnabled;
            }

            _context.SaveChanges();

            return setting;
        }
    }
}