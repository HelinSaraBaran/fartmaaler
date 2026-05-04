using FartmaalerAPI.Models;

namespace FartmaalerAPI.Repositories
{
    public class UsersRepo //: IRepository
    {
        private List<User> m_users = new List<User>();
        private static int nextId = 1;
        public UsersRepo() { } //top konstructor
        public IEnumerable<User> GetAll()
        {
           List<User> users = new List<User>(m_users);
           return users;
        }

        public User? GetById(int id)
        {
            User? user = m_users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return null;
            }
            User userCopy = new User
            {
                Id = user.Id,
                Username = user.Username,
                PasswordHash = user.PasswordHash,
                Role = user.Role
            };
            return userCopy;
        }
        public User Add(User user)
        {
            user.Id = nextId++;
            m_users.Add(user);
            User userCopy = new User
            {
                Id = user.Id,
                Username = user.Username,
                PasswordHash = user.PasswordHash,
                Role = user.Role
            };
            return userCopy;
        }
        public User Delete(int id)
        {
            User? user = m_users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return null;
            }
            m_users.Remove(user);
            User userCopy = new User
            {
                Id = user.Id,
                Username = user.Username,
                PasswordHash = user.PasswordHash,
                Role = user.Role
            };
            return userCopy;
        }
        public User? Update(int id, User updatedUser)
        {
            User? existingUser = m_users.FirstOrDefault(u => u.Id == id);
            if (existingUser == null)
            {
                return null;
            }
            existingUser.Username = updatedUser.Username;
            existingUser.PasswordHash = updatedUser.PasswordHash;
            existingUser.Role = updatedUser.Role;
            User userCopy = new User
            {
                Id = existingUser.Id,
                Username = existingUser.Username,
                PasswordHash = existingUser.PasswordHash,
                Role = existingUser.Role
            };
            return userCopy;
        }



    }
}
