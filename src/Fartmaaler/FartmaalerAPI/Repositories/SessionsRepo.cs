using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories.Interfaces;

namespace FartmaalerAPI.Repositories
{
    public class SessionsRepo : IRepository<Session>
    {
        private readonly AppDbContext _context;

        public SessionsRepo(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Session> GetAll()
        {
            return _context.Sessions.ToList();
        }

        public Session? GetById(int id)
        {
            return _context.Sessions.Find(id);
        }

        public Session Add(Session session)
        {
            _context.Sessions.Add(session);
            _context.SaveChanges();
            return session;
        }

        public Session? Delete(int id)
        {
            var session = _context.Sessions.Find(id);
            if (session == null)
                return null;

            _context.Sessions.Remove(session);
            _context.SaveChanges();
            return session;
        }

        public Session? Update(int id, Session updatedSession)
        {
            var existing = _context.Sessions.Find(id);
            if (existing == null)
                return null;

            existing.GroupId = updatedSession.GroupId;
            existing.CarType = updatedSession.CarType;
            existing.RoadType = updatedSession.RoadType;
            existing.SpeedLimit = updatedSession.SpeedLimit;
            existing.ScalingFactor = updatedSession.ScalingFactor;
            existing.Status = updatedSession.Status;
            existing.EndedAt = updatedSession.EndedAt;

            _context.SaveChanges();
            return existing;
        }
    }
}