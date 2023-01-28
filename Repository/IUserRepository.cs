using HealthTracker.api.Model;
using Microsoft.AspNetCore.Identity;

namespace HealthTracker.api.Repository
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<bool> UpdateUserProfile(User user);
        Task<User> GetByIdentityId(Guid identityId);
    }
}
