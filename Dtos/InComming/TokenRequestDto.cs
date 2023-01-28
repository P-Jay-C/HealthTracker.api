using System.ComponentModel.DataAnnotations;

namespace HealthTracker.api.Dtos.InComming
{
    public class TokenRequestDto
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
