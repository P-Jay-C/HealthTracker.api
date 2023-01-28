using HealthTracker.api.Model;

namespace HealthTracker.api.Repository
{ 
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
        {
            Task<RefreshToken> GetByRefreshTokenAsync(string refreshToken);
            Task<bool> MarkRefreshTokenAsUsedAsync(RefreshToken refreshToken);
        }
}
