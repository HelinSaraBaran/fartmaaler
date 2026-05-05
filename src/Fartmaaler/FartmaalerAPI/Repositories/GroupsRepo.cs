using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories.Interfaces;

namespace FartmaalerAPI.Repositories
{
    // Dette repository håndterer dataadgang for Group
    // Klassen implementerer IRepository<Group>, så den følger samme CRUD struktur
    public class GroupsRepo : IRepository<Group>
    {
        // Database context bruges til at kommunikere med databasen via Entity Framework
        private readonly AppDbContext _context;

        // Constructoren modtager AppDbContext via dependency injection
        public GroupsRepo(AppDbContext context)
        {
            _context = context;
        }

        // Henter alle grupper fra databasen
        public IEnumerable<Group> GetAll()
        {
            return _context.Groups.ToList();
        }

        // Finder en gruppe ud fra id
        // Returnerer null hvis gruppen ikke findes
        public Group? GetById(int id)
        {
            return _context.Groups.Find(id);
        }

        // Tilføjer en ny gruppe til databasen
        public Group Add(Group group)
        {
            _context.Groups.Add(group);
            _context.SaveChanges();
            return group;
        }

        // Sletter en gruppe ud fra id
        // Returnerer den slettede gruppe eller null hvis den ikke findes
        public Group? Delete(int id)
        {
            // Finder først gruppen i databasen
            var group = _context.Groups.Find(id);

            // Hvis gruppen ikke findes, stopper metoden og returnerer null
            if (group == null)
                return null;

            // Fjerner gruppen fra databasen
            _context.Groups.Remove(group);

            // Gemmer ændringen i databasen
            _context.SaveChanges();

            return group;
        }

        // Opdaterer en eksisterende gruppe ud fra id
        // Returnerer den opdaterede gruppe eller null hvis den ikke findes
        public Group? Update(int id, Group updatedGroup)
        {
            // Finder den eksisterende gruppe i databasen
            var existing = _context.Groups.Find(id);

            // Hvis gruppen ikke findes, returneres null
            if (existing == null)
                return null;

            // Opdaterer gruppens felter med de nye værdier
            existing.Name = updatedGroup.Name;
            existing.School = updatedGroup.School;
            existing.IsLocked = updatedGroup.IsLocked;

            // Gemmer ændringerne i databasen
            _context.SaveChanges();

            return existing;
        }
    }
}