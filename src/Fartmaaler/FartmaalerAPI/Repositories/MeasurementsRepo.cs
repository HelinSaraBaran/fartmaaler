using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories.Interfaces;

namespace FartmaalerAPI.Repositories
{
    // Dette repository håndterer data for Measurement (målinger)
    // Bruges til at gemme og hente data fra fartmåleren
    public class MeasurementsRepo : IRepository<Measurement>
    {
        // Database context til kommunikation med databasen
        private readonly AppDbContext _context;

        // Constructor modtager context via dependency injection
        public MeasurementsRepo(AppDbContext context)
        {
            _context = context;
        }

        // Henter alle målinger fra databasen
        public IEnumerable<Measurement> GetAll()
        {
            return _context.Measurements.ToList();
        }

        // Finder en måling ud fra id
        public Measurement? GetById(int id)
        {
            return _context.Measurements.Find(id);
        }

        // Tilføjer en ny måling til databasen
        public Measurement Add(Measurement measurement)
        {
            _context.Measurements.Add(measurement);

            // Gemmer målingen i databasen
            _context.SaveChanges();

            return measurement;
        }

        // Sletter en måling ud fra id
        public Measurement? Delete(int id)
        {
            // Finder målingen først
            var measurement = _context.Measurements.Find(id);

            // Hvis den ikke findes, returneres null
            if (measurement == null)
                return null;

            // Fjerner målingen fra databasen
            _context.Measurements.Remove(measurement);

            // Gemmer ændringen
            _context.SaveChanges();

            return measurement;
        }

        // Update er ikke implementeret
        // Målinger må ikke ændres efter de er gemt (data skal være troværdig)
        public Measurement? Update(int id, Measurement updatedMeasurement)
        {
            throw new NotImplementedException("Målinger kan ikke opdateres");
        }
    }
}