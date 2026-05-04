using FartmaalerAPI.Models;

namespace FartmaalerAPI.Repositories
{
    public class MeasurementsRepo : IRepository<Measurement>
    {
        private List<Measurement> m_measurements = new List<Measurement>();
        private static int nextId = 1;
        public MeasurementsRepo() { } //top konstructor

        public IEnumerable<Measurement> GetAll()
        {
            List<Measurement> measurements = new List<Measurement>(m_measurements);
            return measurements;
        }
        public Measurement? GetById(int id)
        {
            Measurement? measurement = m_measurements.FirstOrDefault(m => m.Id == id);
            if (measurement == null)
            {
                return null;
            }
            Measurement measurementCopy = new Measurement
            {
                Id = measurement.Id,
                SessionId = measurement.SessionId,
                MeasuredSpeed = measurement.MeasuredSpeed,
                SimulatedSpeed = measurement.SimulatedSpeed,
                Time = measurement.Time,
                Distance = measurement.Distance,
                Co2 = measurement.Co2,
                Co2Saved = measurement.Co2Saved,
                CreatedAt = measurement.CreatedAt
            };
            return measurementCopy;
        }
        public Measurement Add(Measurement measurement)
        {
            measurement.Id = nextId++;
            m_measurements.Add(measurement);
            Measurement measurementCopy = new Measurement
            {
                Id = measurement.Id,
                SessionId = measurement.SessionId,
                MeasuredSpeed = measurement.MeasuredSpeed,
                SimulatedSpeed = measurement.SimulatedSpeed,
                Time = measurement.Time,
                Distance = measurement.Distance,
                Co2 = measurement.Co2,
                Co2Saved = measurement.Co2Saved,
                CreatedAt = measurement.CreatedAt
            };
            return measurementCopy;

        }
        public Measurement? Delete(int id)
        {
            Measurement? measurement = m_measurements.FirstOrDefault(m => m.Id == id);
            if (measurement == null)
            {
                return null;
            }
            m_measurements.Remove(measurement);
            Measurement measurementCopy = new Measurement
            {
                Id = measurement.Id,
                SessionId = measurement.SessionId,
                MeasuredSpeed = measurement.MeasuredSpeed,
                SimulatedSpeed = measurement.SimulatedSpeed,
                Time = measurement.Time,
                Distance = measurement.Distance,
                Co2 = measurement.Co2,
                Co2Saved = measurement.Co2Saved,
                CreatedAt = measurement.CreatedAt
            };
            return measurementCopy;
        }


        public Measurement? Update(int id, Measurement updatedMeasurement)
        {
            Measurement? existingMeasurement = m_measurements.FirstOrDefault(m => m.Id == id);
            if (existingMeasurement == null)
            {
                return null;
            }
            existingMeasurement.SessionId = updatedMeasurement.SessionId;
            existingMeasurement.MeasuredSpeed = updatedMeasurement.MeasuredSpeed;
            existingMeasurement.SimulatedSpeed = updatedMeasurement.SimulatedSpeed;
            existingMeasurement.Time = updatedMeasurement.Time;
            existingMeasurement.Distance = updatedMeasurement.Distance;
            existingMeasurement.Co2 = updatedMeasurement.Co2;
            existingMeasurement.Co2Saved = updatedMeasurement.Co2Saved;
            existingMeasurement.CreatedAt = updatedMeasurement.CreatedAt;
            Measurement measurementCopy = new Measurement
            {
                Id = existingMeasurement.Id,
                SessionId = existingMeasurement.SessionId,
                MeasuredSpeed = existingMeasurement.MeasuredSpeed,
                SimulatedSpeed = existingMeasurement.SimulatedSpeed,
                Time = existingMeasurement.Time,
                Distance = existingMeasurement.Distance,
                Co2 = existingMeasurement.Co2,
                Co2Saved = existingMeasurement.Co2Saved,
                CreatedAt = existingMeasurement.CreatedAt
            };
            return measurementCopy;

        }
    }
}
