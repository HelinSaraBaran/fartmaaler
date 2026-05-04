using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories.Interfaces;

namespace FartmaalerAPI.Repositories
{
    public class GroupsRepo : IRepository<Group>
    {
        private readonly AppDbContext _context;

        public GroupsRepo(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Group> GetAll()
        {
            return _context.Groups.ToList();
        }

        public Group? GetById(int id)
        {
            return _context.Groups.Find(id);
        }

        public Group Add(Group group)
        {
            _context.Groups.Add(group);
            _context.SaveChanges();
            return group;
        }

        public Group? Delete(int id)
        {
            var group = _context.Groups.Find(id);
            if (group == null)
                return null;

            _context.Groups.Remove(group);
            _context.SaveChanges();
            return group;
        }

        public Group? Update(int id, Group updatedGroup)
        {
            var existing = _context.Groups.Find(id);
            if (existing == null)
                return null;

            existing.Name = updatedGroup.Name;
            existing.School = updatedGroup.School;
            existing.IsLocked = updatedGroup.IsLocked;

            _context.SaveChanges();
            return existing;
        }
    }
}