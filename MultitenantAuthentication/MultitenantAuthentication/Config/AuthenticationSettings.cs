namespace MultitenantAuthentication.Config
{
    public class AuthenticationSettings
    {
        public string? AuthorityPrefix { get; set; }
        public string? Metadata { get; set; }
        public string? Audience { get; set; }
        public string? AllowedIssuersRegex { get; set; }
    }
}
