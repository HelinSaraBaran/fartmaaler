using FartmaalerAPI.Models;

namespace FartmaalerAPI.Repositories
{
    public class GroupsRepo : IRepository<Group>
    {
        private List<Group> m_groups = new List<Group>();
        private static int nextId = 1;
        public GroupsRepo() { } //top konstructor

        public IEnumerable<Group> GetAll()
        {
            List<Group> groups = new List<Group>(m_groups);
            return groups;
        }

        public Group? GetById(int id)
        {
            Group group = m_groups.FirstOrDefault(g => g.Id == id);
            if (group == null)
            {
                return null;
            }
            Group groupCopy = new Group
            {
                Id = group.Id,
                Name = group.Name,
                School = group.School,
                IsLocked = group.IsLocked
            };
            return groupCopy;
        }

        public Group? Add(Group group)

        {
            group.Id = nextId++;
            m_groups.Add(group);
            Group groupCopy = new Group
            {
                Id = nextId++,
                Name = group.Name,
                School = group.School,
                IsLocked = group.IsLocked
            };
            return groupCopy;
        }
        
      public Group? Delete(int id)
        {
            Group group = m_groups.FirstOrDefault(g => g.Id == id);
            if (group == null)
            {
                return null;
            }
            m_groups.Remove(group);
            Group groupCopy = new Group
            {
                Id = group.Id,
                Name = group.Name,
                School = group.School,
                IsLocked = group.IsLocked
            };
            return groupCopy;
        }   
        public Group Update(int id, Group updatedGroup)
        {
            Group group = m_groups.FirstOrDefault(g => g.Id == id);
            if (group == null)
            {
                return null;
            }
            group.Name = updatedGroup.Name;
            group.School = updatedGroup.School;
            group.IsLocked = updatedGroup.IsLocked;
            Group groupCopy = new Group
            {
                Id = group.Id,
                Name = group.Name,
                School = group.School,
                IsLocked = group.IsLocked
            };
            return groupCopy;
        }

    }
}
