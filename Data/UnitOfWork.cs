using HealthTracker.api.Repository;

namespace HealthTracker.api.Data
{
    public class UnitOfWork:IUnitOfWork,IDisposable
    {
        private readonly AppDbContext _context;
        private readonly ILogger _logger;

        public IUserRepository User { get; private set; }
        public IRefreshTokenRepository RefreshToken { get; private set; }

        public UnitOfWork(AppDbContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger("db_logs");

            User = new UserRepository(context, logger: _logger);
            RefreshToken = new RefreshTokenRepository(context, logger: _logger);
        }

        public async Task CompleteAsync()
        {
            await _context.SaveChangesAsync();
        }


        public void Dispose()
        {
            _context.Dispose();
        }
    }

}
