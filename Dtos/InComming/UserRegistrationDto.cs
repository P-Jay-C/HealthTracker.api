using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace HealthTracker.api.Dtos.InComming
{
    public class UserRegistrationDto
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
