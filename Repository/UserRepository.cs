using HealthTracker.api.Data;
using HealthTracker.api.Model;
using Microsoft.EntityFrameworkCore;

namespace HealthTracker.api.Repository
{
    public class UserRepository:GenericRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context, ILogger logger) : base(context,logger)
        {

        }

        public override async Task<IEnumerable<User>> All()
        {
            try
            {
                return await dbSet.Where(x => x.status == 1)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Repo} method has generated an error",typeof(UserRepository));
                return Enumerable.Empty<User>();
            }
        }
    }
}
