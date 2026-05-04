using FartmaalerAPI.Models;

namespace FartmaalerAPI.Repositories
{
    public class SessionsRepo
    {
        private List<Session> m_sessions = new List<Session>();
        private static int nextID = 1;

        public IEnumerable<Session> GetAll()
        {
            List<Session> sessions = new List<Session>(m_sessions);
            return sessions;
        }

        public Session? GetById(int id)
        {
            Session? session = m_sessions.FirstOrDefault(s => s.Id == id);

            if (session == null)
            {
                return null;
            }

            Session sessionCopy = new Session
            {
                Id = session.Id,
                GroupId = session.GroupId,
                Group = session.Group,
                CarType = session.CarType,
                RoadType = session.RoadType,
                SpeedLimit = session.SpeedLimit,
                ScalingFactor = session.ScalingFactor,
                Status = session.Status,
                CreatedAt = session.CreatedAt,
                EndedAt = session.EndedAt
            };

            return sessionCopy;
        }

        public Session Add(Session session)
        {
            session.Id = nextID++;
            m_sessions.Add(session);

            Session sessionCopy = new Session
            {
                Id = session.Id,
                GroupId = session.GroupId,
                Group = session.Group,
                CarType = session.CarType,
                RoadType = session.RoadType,
                SpeedLimit = session.SpeedLimit,
                ScalingFactor = session.ScalingFactor,
                Status = session.Status,
                CreatedAt = session.CreatedAt,
                EndedAt = session.EndedAt
            };

            return sessionCopy;
        }

        public Session? Delete(int id)
        {
            Session? session = m_sessions.FirstOrDefault(s => s.Id == id);

            if (session == null)
            {
                return null;
            }

            m_sessions.Remove(session);

            Session sessionCopy = new Session
            {
                Id = session.Id,
                GroupId = session.GroupId,
                Group = session.Group,
                CarType = session.CarType,
                RoadType = session.RoadType,
                SpeedLimit = session.SpeedLimit,
                ScalingFactor = session.ScalingFactor,
                Status = session.Status,
                CreatedAt = session.CreatedAt,
                EndedAt = session.EndedAt
            };

            return sessionCopy;
        }

        public Session? Update(int id, Session updatedSession)
        {
            Session? existingSession = m_sessions.FirstOrDefault(s => s.Id == id);

            if (existingSession == null)
            {
                return null;
            }

            existingSession.GroupId = updatedSession.GroupId;
            existingSession.Group = updatedSession.Group;
            existingSession.CarType = updatedSession.CarType;
            existingSession.RoadType = updatedSession.RoadType;
            existingSession.SpeedLimit = updatedSession.SpeedLimit;
            existingSession.ScalingFactor = updatedSession.ScalingFactor;
            existingSession.Status = updatedSession.Status;
            existingSession.CreatedAt = updatedSession.CreatedAt;
            existingSession.EndedAt = updatedSession.EndedAt;

            Session sessionCopy = new Session
            {
                Id = existingSession.Id,
                GroupId = existingSession.GroupId,
                Group = existingSession.Group,
                CarType = existingSession.CarType,
                RoadType = existingSession.RoadType,
                SpeedLimit = existingSession.SpeedLimit,
                ScalingFactor = existingSession.ScalingFactor,
                Status = existingSession.Status,
                CreatedAt = existingSession.CreatedAt,
                EndedAt = existingSession.EndedAt
            };

            return sessionCopy;
        }
    }
}