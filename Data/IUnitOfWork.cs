using HealthTracker.api.Repository;

namespace HealthTracker.api.Data
{
    public interface IUnitOfWork
    {
        IUserRepository User { get; }

        Task CompleteAsync();
    }
}
