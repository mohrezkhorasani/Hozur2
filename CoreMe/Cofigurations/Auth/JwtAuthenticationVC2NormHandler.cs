using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace CoreMe.Cofigurations.Auth
{
    public class JwtAuthenticationVC2NormHandler : AuthenticationHandler<JwtAuthenticationVC2NormOptions>
    {
        public JwtAuthenticationVC2NormHandler(
            IOptionsMonitor<JwtAuthenticationVC2NormOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("vc2AuthNorm"))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            string token = Request.Headers["vc2AuthNorm"];
            if (string.IsNullOrEmpty(token))
            {
                return Task.FromResult(AuthenticateResult.Fail("Token is empty."));
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(Options.SecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),

                    ValidateIssuer = true,
                    ValidIssuer = Options.ValidIssuer,

                    ValidateAudience = true,
                    ValidAudience = Options.ValidAudience,

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,

                    // Optional: Validate the signing key
                    RequireSignedTokens = true,
                    RequireExpirationTime = true
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch (SecurityTokenException ex)
            {
                Logger.LogError("JWT Security error: {Error}", ex.Message);
                return Task.FromResult(AuthenticateResult.Fail($"Invalid token: {ex.Message}"));
            }
            catch (Exception ex)
            {
                Logger.LogError("JWT Authentication error: {Error}", ex.Message);
                return Task.FromResult(AuthenticateResult.Fail($"Authentication failed: {ex.Message}"));
            }
        }
    }
}