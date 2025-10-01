using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public static class JwtHelper
{
    public static Dictionary<string, string> GetAllClaims(string jwtToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var claimsDict = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(jwtToken))
            return claimsDict;

        if (jwtToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            jwtToken = jwtToken.Substring("Bearer ".Length).Trim();

        if (handler.CanReadToken(jwtToken))
        {
            var token = handler.ReadJwtToken(jwtToken);

            foreach (var claim in token.Claims)
            {
                // در صورتی که کلید تکراری وجود داشت، آخرین مقدار جایگزین قبلی میشه
                claimsDict[claim.Type] = claim.Value;
            }
        }
        else
        {
            Console.WriteLine("Invalid JWT token.");
        }

        return claimsDict;
    }
}
