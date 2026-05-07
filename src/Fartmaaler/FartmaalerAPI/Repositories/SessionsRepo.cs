using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories.Interfaces;

namespace FartmaalerAPI.Repositories
{
    // Dette repository håndterer data for Session
    // En session er et samlet forsøg i fartmåleren
    public class SessionsRepo : IRepository<Session>
    {
        // Database context til kommunikation med databasen
        private readonly AppDbContext _context;

        // Constructor modtager context via dependency injection
        public SessionsRepo(AppDbContext context)
        {
            _context = context;
        }

        // Henter alle sessions fra databasen
        public IEnumerable<Session> GetAll()
        {
            return _context.Sessions.ToList();
        }

        // Finder en session ud fra id
        public Session? GetById(int id)
        {
            return _context.Sessions.Find(id);
        }

        // Tilføjer en ny session til databasen
        public Session Add(Session session)
        {
            _context.Sessions.Add(session);

            // Gemmer sessionen i databasen
            _context.SaveChanges();

            return session;
        }

        // Sletter en session ud fra id
        public Session? Delete(int id)
        {
            // Finder sessionen først
            var session = _context.Sessions.Find(id);

            // Hvis den ikke findes returneres null
            if (session == null)
                return null;

            // Fjerner sessionen fra databasen
            _context.Sessions.Remove(session);

            // Gemmer ændringerne
            _context.SaveChanges();

            return session;
        }

        // Opdaterer en eksisterende session
        public Session? Update(int id, Session updatedSession)
        {
            // Finder eksisterende session
            var existing = _context.Sessions.Find(id);

            // Hvis sessionen ikke findes returneres null
            if (existing == null)
                return null;

            // Opdaterer relevante felter
            existing.GroupId = updatedSession.GroupId;
            existing.CarType = updatedSession.CarType;
            existing.RoadType = updatedSession.RoadType;
            existing.SpeedLimit = updatedSession.SpeedLimit;
            existing.ScalingFactor = updatedSession.ScalingFactor;
            existing.Status = updatedSession.Status;
            existing.EndedAt = updatedSession.EndedAt;

            // CreatedAt opdateres ikke
            // Starttidspunktet for sessionen skal bevares

            // Gemmer ændringerne
            _context.SaveChanges();

            return existing;
        }
    }
}