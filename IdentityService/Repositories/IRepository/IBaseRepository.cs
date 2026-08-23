namespace IdentityService.Repositories.IRepository
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T> AddAsync(T entity);

        void Update(T entity);

        void Delete(T entity);

        Task<T?> GetByIdAsync(Guid id);

        Task<List<T>> GetAllAsync();
    }
}