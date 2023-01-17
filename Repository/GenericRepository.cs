using HealthTracker.api.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthTracker.api.Repository
{
    public class GenericRepository<T>:IGenericRepository<T> where T : class
    {
        public readonly AppDbContext _context;
        public readonly ILogger _logger;
        internal DbSet<T> dbSet;

        protected GenericRepository(AppDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
            this.dbSet = _context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> All()
        {
            return await dbSet.ToListAsync();
        }

        public virtual async Task<T> GetById(Guid id)
        {
            return await dbSet.FindAsync(id);
        }

        public virtual async Task<bool> Add(T entity)
        {
            await dbSet.AddAsync(entity);

            return true;
        }

        public Task<bool> Delete(Guid id, string UserId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Update(T entity)
        {
            throw new NotImplementedException();
        }
    }
}
