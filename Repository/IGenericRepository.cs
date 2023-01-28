namespace HealthTracker.api.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        // Get all entities
        Task<IEnumerable<T>> All();

        // Get Specific entity based on id
        Task<T> GetById(Guid id);

        Task<bool> Add(T entity);

        Task<bool> Delete(Guid id, string UserId);

        Task<bool> Update(T entity); 
    }
}
