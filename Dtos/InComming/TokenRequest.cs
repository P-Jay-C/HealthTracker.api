using System.ComponentModel.DataAnnotations;

namespace HealthTracker.api.Dtos.InComming
{
    public class TokenRequest
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
