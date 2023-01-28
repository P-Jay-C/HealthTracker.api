using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace HealthTracker.api.Model
{
    public class RefreshToken:BaseEntity
    {
        public string UserId { get; set; } // user id when logged in
        public string Token { get; set; }
        public string JwtId { get; set; } // id generated when jwt id is requested

        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime ExpiryDate { get; set; }

        [ForeignKey(nameof(UserId))]
        public IdentityUser User { get; set; }
    }
}
