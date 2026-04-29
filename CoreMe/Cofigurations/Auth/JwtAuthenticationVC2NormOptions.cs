using Microsoft.AspNetCore.Authentication;

namespace CoreMe.Cofigurations.Auth
{
    public class JwtAuthenticationVC2NormOptions : AuthenticationSchemeOptions
    {
        public string ValidIssuer { get; set; } = "core.hekmateislami.com";
        public string ValidAudience { get; set; } = "vc2.hekmateislami.com";
        public string SecretKey { get; set; } = "a1b32c5e-8f1d-5a3c-8d5e-2f6a5b4c9d0e";
    }
}