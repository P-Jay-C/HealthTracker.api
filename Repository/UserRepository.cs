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

        public async Task<User> GetByIdentityId(Guid identityId)
        {
            try
            {
                return await dbSet.Where(x => x.status == 1 && x.IdentityId == identityId)
                    .FirstOrDefaultAsync();
            }

            catch (Exception e)
            {
                _logger.LogError(e, "{Repo} GetByIdentityId has generated an error", typeof(UserRepository));
                return null;
            }
        }

        public async Task<bool> UpdateUserProfile(User user)
        {
            try
            {
                var existingUser = await dbSet.Where(x => x.status == 1 && x.Id == user.Id)
                    .FirstOrDefaultAsync();

                if (existingUser == null) return false;

                existingUser.FirstName = user.FirstName;
                existingUser.LastName =     user.LastName;
                existingUser.Email = user.Email;
                existingUser.Phone = user.Phone;
                existingUser.DateOfBirth = user.DateOfBirth;
                existingUser.MobileNumber = user.MobileNumber;
                existingUser.Sex = user.Sex;
                existingUser.UpdatedDateTime = DateTime.UtcNow;

                return true;
            }

            catch (Exception e)
            {
                _logger.LogError(e, "{Repo} UpdateUserProfile has generated an error", typeof(UserRepository));
                return false;
            }
        }
    }
}
