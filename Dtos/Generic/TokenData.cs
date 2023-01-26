using System.ComponentModel.DataAnnotations;

namespace HealthTracker.api.Dtos.Generic
{
    public class TokenData
    {
        public string JwTToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
