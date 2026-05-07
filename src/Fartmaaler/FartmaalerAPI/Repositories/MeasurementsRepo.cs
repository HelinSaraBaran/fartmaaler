using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories.Interfaces;

namespace FartmaalerAPI.Repositories
{
    public class MeasurementsRepo : IRepository<Measurement>
    {
        private readonly AppDbContext _context;

        public MeasurementsRepo(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Measurement> GetAll()
        {
            return _context.Measurements.ToList();
        }

        public Measurement? GetById(int id)
        {
            return _context.Measurements.Find(id);
        }

        public IEnumerable<Measurement> GetBySessionId(int sessionId)
        {
            return _context.Measurements
                .Where(m => m.SessionId == sessionId)
                .ToList();
        }

        public Measurement Add(Measurement measurement)
        {
            _context.Measurements.Add(measurement);
            _context.SaveChanges();

            return measurement;
        }

        public Measurement? Delete(int id)
        {
            var measurement = _context.Measurements.Find(id);

            if (measurement == null)
                return null;

            _context.Measurements.Remove(measurement);
            _context.SaveChanges();

            return measurement;
        }

        public Measurement? Update(int id, Measurement updatedMeasurement)
        {
            throw new NotImplementedException("Målinger kan ikke opdateres");
        }
    }
}