namespace TimeLogAuthPOC.Config
{
    public class JWTConfig
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Secret { get; set; }
        public int ExpirationInDays { get; set; }
    }
}
