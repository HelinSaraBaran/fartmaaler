namespace FartmaalerAPI.Repositories.Interfaces
{
    // Dette interface definerer standard metoder til datahåndtering (CRUD)
    // Det bruges til at sikre ens struktur i alle repositories
    public interface IRepository<T>
    {
        // Returnerer alle elementer af typen T
        IEnumerable<T> GetAll();

        // Finder et element ud fra id
        // Returnerer null hvis det ikke findes
        T? GetById(int id);

        // Tilføjer et nyt element og returnerer det oprettede objekt
        T Add(T item);

        // Sletter et element ud fra id
        // Returnerer det slettede objekt eller null hvis det ikke findes
        T? Delete(int id);

        // Opdaterer et eksisterende element
        // Returnerer det opdaterede objekt eller null hvis det ikke findes
        T? Update(int id, T updatedItem);
    }
}