using Application.DTO.Generals;
using Application.Interfaces;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Presistence.Entities;
using System.Security.Claims;

public interface IGeneralHeadersService
{
    Task<GeneralHeaders> GetHeadersAsync(HttpContext httpContext);
}

public class GeneralHeadersService : IGeneralHeadersService
{
    private readonly IDBContext _db; // DbContext شما

    private readonly IMemoryCache _cache;

    public GeneralHeadersService(IDBContext dbContext, IMemoryCache cache)
    {
        _db = dbContext;
        _cache = cache;
    }

    public async Task<GeneralHeaders> GetHeadersAsync(HttpContext httpContext)
    {
        var headers = new GeneralHeaders();

        // مقداردهی از هدرهای HTTP
        headers.Agent = httpContext.Request.Headers["User-Agent"].ToString();
        headers.RequestedIP = httpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        headers.RequestedDevice = GetDevice(httpContext.Request.Headers["User-Agent"].ToString());
        headers.RequestedPage = httpContext.Request.Path.ToString();
        headers.Referer = httpContext.Request.Headers["Referer"].ToString();
        headers.RefererGoogle = httpContext.Request.Headers["Referer-Google"].ToString();
        headers.JWTUserToken = httpContext.Request.Headers["vc2AuthNorm"].ToString();

        try
        {
            headers.JWTMeetingUserToken = httpContext.Request.Headers["Authorization"].ToString();
        }
        catch (Exception e)
        {
            headers.JWTMeetingUserToken = "";
        }
        try
        {
            headers.JWTMeetingToken = httpContext.Request.Headers["Meet-Secret"].ToString();
        }
        catch (Exception e)
        {
            headers.JWTMeetingToken = "";
        }


        // مقداردهی Guid
        if (Guid.TryParse(httpContext.Request.Headers["Identification-Code"].ToString(), out Guid guid))
        {
            headers.IdentificationCode = guid;
        }
        else
        {
            headers.IdentificationCode = Guid.NewGuid();
        }

        // مقداردهی از JWT
        var user = httpContext.User;
        if (user.Identity.IsAuthenticated)
        {
            var userId = user.FindFirst("UserID")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // تلاش برای دریافت از کش
                string cacheKey = $"User_{userId}";

                if (!_cache.TryGetValue(cacheKey, out UserTBL cachedUser))
                {
                    // اگر در کش نبود، از دیتابیس بخوان
                    cachedUser = await _db.UserTBLs
                        .FirstOrDefaultAsync(u => u.UUID == userId);

                    // ذخیره در کش برای 5 دقیقه
                    if (cachedUser != null)
                    {
                        _cache.Set(cacheKey, cachedUser, TimeSpan.FromMinutes(5));
                    }
                }

                headers.UserTBL = cachedUser;
            }
        }

        return headers;
    }

    private string GetDevice(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return "Unknown";

        userAgent = userAgent.ToLower();

        if (userAgent.Contains("mobile") || userAgent.Contains("android") || userAgent.Contains("iphone"))
            return "Mobile";
        else if (userAgent.Contains("tablet") || userAgent.Contains("ipad"))
            return "Tablet";
        else
            return "Desktop";
    }
}