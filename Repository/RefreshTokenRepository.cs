using HealthTracker.api.Data;
using HealthTracker.api.Model;
using Microsoft.EntityFrameworkCore;

namespace HealthTracker.api.Repository
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(AppDbContext context, ILogger logger) : base(context, logger)
        {

        }

        public override async Task<IEnumerable<RefreshToken>> All()
        {
            try
            {
                return await dbSet.Where(x => x.status == 1)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Repo} method has generated an error", typeof(RefreshTokenRepository));
                return Enumerable.Empty<RefreshToken>();
            }
        }
    }
}