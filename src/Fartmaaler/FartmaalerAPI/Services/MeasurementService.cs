using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;

namespace FartmaalerAPI.Services
{
    // Denne service håndterer beregning og oprettelse af målinger
    public class MeasurementService
    {
        private readonly AppDbContext _context;
        private readonly MeasurementsRepo _repo;

        private const double DistanceMeters = 1;

        public MeasurementService(AppDbContext context, MeasurementsRepo repo)
        {
            _context = context;
            _repo = repo;
        }

        // Returnerer CO2 faktor ud fra biltype
        private double GetCo2Factor(string carType)
        {
            return carType?.ToLower() switch
            {
                "benzin lille" => 120,
                "benzin stor" => 180,
                "diesel" => 140,
                "hybrid" => 90,
                _ => 120
            };
        }

        // Opretter en måling ud fra session og tid
        public Measurement? CreateMeasurement(int sessionId, double timeSeconds)
        {
            if (sessionId <= 0 || timeSeconds <= 0)
                return null;

            Session? session = _context.Sessions.FirstOrDefault(session => session.Id == sessionId);

            if (session == null)
                return null;

            if (session.Status?.ToLower() == "ended")
                return null;

            double speedMetersPerSecond = DistanceMeters / timeSeconds;
            double measuredSpeedKmh = speedMetersPerSecond * 3.6;

            // User story 9: målt hastighed ganges med sessionens scaling factor
            double simulatedSpeedKmh =
            measuredSpeedKmh * session.ScalingFactor;
            string status;

            if (simulatedSpeedKmh > session.SpeedLimit)
                status = "Too fast";
            else if (simulatedSpeedKmh < session.SpeedLimit)
                status = "Under limit";
            else
                status = "On limit";

            double co2Factor = GetCo2Factor(session.CarType);

            Measurement measurement = new Measurement
            {
                SessionId = sessionId,
                Distance = DistanceMeters,
                Time = timeSeconds,
                MeasuredSpeed = Math.Round(measuredSpeedKmh, 2),
                SimulatedSpeed = Math.Round(simulatedSpeedKmh, 2),
                SpeedLimit = session.SpeedLimit,
                Status = status,
                Co2 = Math.Round((co2Factor * simulatedSpeedKmh) / 1000, 2),
                Co2Saved = Math.Round((co2Factor * (session.SpeedLimit - simulatedSpeedKmh)) / 1000, 2),
                CreatedAt = DateTime.Now
            };

            return _repo.Add(measurement);
        }
    }
}