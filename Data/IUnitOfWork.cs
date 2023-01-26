using HealthTracker.api.Model;
using HealthTracker.api.Repository;

namespace HealthTracker.api.Data
{
    public interface IUnitOfWork
    {
        IUserRepository User { get; } 
        IRefreshTokenRepository RefreshToken { get; }

        Task CompleteAsync();
    }
}
