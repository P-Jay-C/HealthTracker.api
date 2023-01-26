namespace HealthTracker.api.Configuration
{
    public class JwtConfig
    {
        public string Secret { get; set; }
        public TimeSpan ExpiryDateFrame { get; set; }   
    }
}
