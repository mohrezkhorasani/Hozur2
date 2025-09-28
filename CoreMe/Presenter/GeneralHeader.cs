using Application.DTO.Generals;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace Presenter
{
    public static class GeneralHeader
    {
        public static GeneralHeaders GetGeneralHeaders(HttpRequest request)
        {
            try
            {

                Guid guid = GetIdentificationCode(request);
                Guid guidJWT = Guid.Parse(GetFromJWT(request.Headers["Authorization"].ToString(), "guid"));

                return new GeneralHeaders
                {
                    Agent = request.Headers["User-Agent"].ToString(),
                    Token = request.Headers["Authorization"].ToString(),
                    RequestedPage = request.Path.ToString(),
                    RequestedIP = request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    RequestedDevice = request.Headers["sec-ch-ua-platform"].ToString(), // Assuming User-Agent for device
                    IdentificationCode = guid,
                    JWTIdentificationCode = guidJWT,
                    Referer = request.Headers["Referer"].ToString(),
                    RefererGoogle = request.Headers["Referer"].ToString().Contains("google") ? request.Headers["Referer"].ToString() : null
                };
            }
            catch (Exception ex)
            {
                string ip = "";
                if (request.HttpContext.Connection.RemoteIpAddress != null)
                {
                    ip = request.HttpContext.Connection.RemoteIpAddress.ToString();
                }
                return new GeneralHeaders{
                    RequestedIP= ip,
                    Agent= request.Headers["User-Agent"].ToString(),
                    Referer = request.Headers["Referer"].ToString(),
                    RefererGoogle = request.Headers["Referer"].ToString().Contains("google") ? request.Headers["Referer"].ToString() : null,
                    RequestedPage = request.Path.ToString(),
                    RequestedDevice = request.Headers["sec-ch-ua-platform"].ToString(), // Assuming User-Agent for device

                };
            }

        }
        public static string GetFromJWT(string jwtToken, string claimType)
        {
            var handler = new JwtSecurityTokenHandler();
            if (jwtToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                jwtToken = jwtToken.Substring("Bearer ".Length).Trim();
            }

            if (handler.CanReadToken(jwtToken))
            {
                var token = handler.ReadJwtToken(jwtToken);

                // Find the claim with the specified type
                var guidClaim = token.Claims.FirstOrDefault(c => c.Type == claimType);

                if (guidClaim != null && Guid.TryParse(guidClaim.Value, out var guid))
                {
                    return guid.ToString(); // Return the GUID value
                }

                Console.WriteLine($"No valid GUID found for claim type: {claimType}");
                return null;
            }
            else
            {
                Console.WriteLine("Invalid JWT token.");
                return null;
            }
        }
        public static Guid GetIdentificationCode(HttpRequest request)
        {
                try
                {
                    var headerValue = request.Headers["IdentificationCode"].ToString();
                    if (string.IsNullOrEmpty(headerValue))
                        return Guid.Empty;
                    return new Guid(headerValue);
                }
                catch
                {
                return Guid.Empty;
            }   
        }
     
        public static string GetIdentificationCodeNoLogin(HttpRequest request)
        {
            try
            {
                var headerValue = request.Headers["AgentCode"].ToString();
                if (string.IsNullOrEmpty(headerValue))
                    return string.Empty;
                return new string(headerValue);
            }
            catch
            {
                throw;
            }
        }
        public static string GetGU_ASTokenID(HttpRequest request)
        {
            try
            {
                var headerValue = request.Headers["GUID-Token"].ToString();
                if (string.IsNullOrEmpty(headerValue))
                    return string.Empty;
                return new string(headerValue);
            }
            catch
            {
                throw;
            }
        }
        public static string GET_REQUEST_PAGE(HttpRequest request)
        {
            try
            {
                var headerValue = request.Headers["RequestedPage"].ToString();
                if (string.IsNullOrEmpty(headerValue))
                    return string.Empty;
                return new string(headerValue);
            }
            catch
            {
                throw;
            }
        }
        public static string GET_REQUEST_AGENT(HttpRequest request)
        {
            try
            {
                var headerValue = request.Headers["RequestedAgent"].ToString();
                if (string.IsNullOrEmpty(headerValue))
                    return string.Empty;
                return new string(headerValue);
            }
            catch
            {
                throw;
            }
        }

        public static string GetClientIPAddress(HttpContext context)
        {
            string ip = string.Empty;
            if (!string.IsNullOrEmpty(context.Request.Headers["X-Forwarded-For"]))
            {
                ip = context.Request.Headers["X-Forwarded-For"];
            }
            else
            {
                ip = context.Request.HttpContext.Features.Get<IHttpConnectionFeature>().RemoteIpAddress.ToString();
            }
            return ip;
        }
    }
}
