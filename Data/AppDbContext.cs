using HealthTracker.api.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthTracker.api.Data
{
    public class AppDbContext:IdentityDbContext
    {
        public DbSet<User> Users { get; set; } 
        public  virtual  DbSet<RefreshToken> RefreshTokens { get; set; } 

        public AppDbContext(DbContextOptions<AppDbContext> options) :
            base(options)
        { }

        

    }
}
