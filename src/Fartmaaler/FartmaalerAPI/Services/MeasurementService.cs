using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;

namespace FartmaalerAPI.Services
{
    public class MeasurementService
    {
        private readonly AppDbContext _context;
        private readonly MeasurementsRepo _repo;

        private const double DistanceMeters = 5.0;
        private const double ToyCarReferenceMaxKmh = 18.0;

        public MeasurementService(AppDbContext context, MeasurementsRepo repo)
        {
            _context = context;
            _repo = repo;
        }

        public Measurement? CreateMeasurement(int sessionId, double timeSeconds)
        {
            if (sessionId <= 0 || timeSeconds <= 0)
                return null;

            var session = _context.Sessions.FirstOrDefault(s => s.Id == sessionId);

            if (session == null)
                return null;

            if (session.Status?.ToLower() == "ended")
                return null;

            double speedMetersPerSecond = DistanceMeters / timeSeconds;
            double measuredSpeedKmh = speedMetersPerSecond * 3.6;

            double simulatedSpeedKmh =
                measuredSpeedKmh / ToyCarReferenceMaxKmh * session.SpeedLimit;

            string status;

            if (simulatedSpeedKmh > session.SpeedLimit)
                status = "Too fast";
            else if (simulatedSpeedKmh < session.SpeedLimit)
                status = "Under limit";
            else
                status = "On limit";

            Measurement measurement = new Measurement
            {
                SessionId = sessionId,
                Distance = DistanceMeters,
                Time = timeSeconds,
                MeasuredSpeed = Math.Round(measuredSpeedKmh, 2),
                SimulatedSpeed = Math.Round(simulatedSpeedKmh, 2),
                SpeedLimit = session.SpeedLimit,
                Status = status,
                Co2 = 0,
                Co2Saved = 0,
                CreatedAt = DateTime.Now
            };

            return _repo.Add(measurement);
        }
    }
}