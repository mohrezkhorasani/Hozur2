using Application.DTO.Generals;
using Application.General;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;

namespace SafaretoWeb
{
    public static class GeneralHeader
    {


        public async static Task<IGeneralHeaders> ReturnHeader(IHttpContextAccessor h)
        {
            var request = h.HttpContext.Request;
            Guid guid = Guid.Empty;
            string token = null;
            try
            {
                string g = null;
                if (h.HttpContext.Request.Cookies.TryGetValue(UserAuth.Key_Guid, out g))
                {
                    guid = Guid.Parse(g);
                }
                string jwt = null;
                if (h.HttpContext.Request.Cookies.TryGetValue(UserAuth.Key_Token, out jwt))
                {
                    token = jwt;
                }
                    
            }
            catch (Exception e)
            {
                _ = TelegramHelper.SendMessage("خطای LocalStorage");
                _ = TelegramHelper.LogExceptionAsync(e);
            }
            return new GeneralHeaders
            {
                IdentificationCode = guid,
                //Token = string.IsNullOrEmpty(token) ? null : token,
                Agent = request == null ? "" : request.Headers.UserAgent,
                Referer = request == null ? "" : request.Headers.Referer,
                RequestedDevice = request == null ? "" : request.Headers.Origin,
                RequestedIP = request == null ? "" : request.Host.ToString(),
            };
        }

        public static IGeneralHeaders ReturnHeader(HttpRequest request)
        {
            Guid guid = Guid.Empty;
            var g = GeneralHeader.GetCookie(request, "User-Guid");
            if (!string.IsNullOrEmpty(g))
            {
                guid = Guid.Parse(g);
            }
            string jwt = "";
            jwt = GetCookie(request, "Token");
            return new GeneralHeaders
            {
                IdentificationCode = guid,
                //Token = jwt
            };
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
        private static string GetCookie(HttpRequest request, string key)
        {
            string cookieValue = "";
            var cookies = request.Cookies;

            if (cookies.TryGetValue(key, out var value))
            {
                cookieValue = value;
                return cookieValue;
            }
            else
            {
                cookieValue = "Cookie not found";
            }
            return null;
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


    }
    public static class LocalStorageService
    {
        private static IJSRuntime _jsRuntime;

        public static void Initialize(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        // Store token
        public static async Task SetTokenAsync(string token)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
        }

        // Get token
        public static async Task<string> GetTokenAsync()
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
        }

        // Remove token
        public static async Task RemoveTokenAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
        }

        // Check if token exists
        public static async Task<bool> HasTokenAsync()
        {
            var token = await GetTokenAsync();
            return !string.IsNullOrEmpty(token);
        }

        // Store any value
        public static async Task SetItemAsync(string key, string value)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
        }

        // Get any value
        public static async Task<string> GetItemAsync(string key)
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
        }

        // Remove any item
        public static async Task RemoveItemAsync(string key)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }

        // Clear all
        public static async Task ClearAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.clear");
        }
    }
}
