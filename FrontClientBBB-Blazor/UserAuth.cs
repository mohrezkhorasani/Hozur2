using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace SafaretoWeb
{
    public class UserAuth
    {
        public static string Key_Token = "t2";
        public static string Key_Guid = "g1";
        public static string Key_Name = "n1";

        public string Token { get; set; }
        public string GUID { get; set; }
        public string Avatar { get; set; }
        public bool NotificationsEnabled { get; set; }
        public string Name { get; set; }
        public async static Task<UserAuth> ReturnUser(ProtectedLocalStorage ls)
        {
            try
            {
                var t2 = await ls.GetAsync<string>(UserAuth.Key_Token);

                if (!t2.Success)
                {
                    await ls.SetAsync("fyuocuk", "you");
                    return null;
                }
                return new UserAuth
                {
                    Token = (await ls.GetAsync<string>(Key_Token)).Value,
                    GUID = (await ls.GetAsync<string>(Key_Guid)).Value,
                    Name = (await ls.GetAsync<string>(Key_Name)).Value
                };
            }
            catch (Exception e)
            {
                return null;
            }

        }
        public async static Task<UserAuth> ReturnUser(IHttpContextAccessor h)
        {
            try
            {
                if (h.HttpContext == null)
                {
                    return null;
                }
                if (h.HttpContext.Request.Cookies.Any(e => e.Key == UserAuth.Key_Token) && h.HttpContext.Request.Cookies.Any(e => e.Key == UserAuth.Key_Guid))
                {
                    return new UserAuth
                    {
                        Token= h.HttpContext?.Request?.Cookies[UserAuth.Key_Token]?.ToString(),
                        GUID= h.HttpContext.Request.Cookies[UserAuth.Key_Guid].ToString()
                    };
                }
                return null;
            }
            catch (Exception e)
            {
                return null;
            }

        }
    }
}
