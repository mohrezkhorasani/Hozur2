using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace NajmBridge.Presenter
{
    public static class JwtValidationMiddleware
    {
        public static async Task ValidateIdentificationCode(HttpContext context, Func<Task> next)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint == null || endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>() == null)
            {
                await next();
                return;
            }

            var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            var identificationCodeHeader = context.Request.Headers["IdentificationCode"].FirstOrDefault();

            if (!string.IsNullOrEmpty(authorizationHeader) && !string.IsNullOrEmpty(identificationCodeHeader))
            {
                if (authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    authorizationHeader = authorizationHeader.Substring("Bearer ".Length).Trim();
                }

                var handler = new JwtSecurityTokenHandler();

                if (handler.CanReadToken(authorizationHeader))
                {
                    var token = handler.ReadJwtToken(authorizationHeader);

                    var guidClaim = token.Claims.FirstOrDefault(c => c.Type == "guid");
                    if (guidClaim != null && Guid.TryParse(guidClaim.Value, out var jwtGuid))
                    {
                        if (!jwtGuid.ToString().Equals(identificationCodeHeader, StringComparison.OrdinalIgnoreCase))
                        {
                            context.Response.StatusCode = 403;
                            await context.Response.WriteAsync("Your access forbidden! that's all we know");
                            return;
                        }
                    }
                }
            }

            await next();
        }
    }

}
