namespace FartmaalerAPI.Repositories
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        T? GetById(int id);
        T Add(T item);
        T? Delete(int id);
        T? Update(int id, T updatedItem);
    }
}