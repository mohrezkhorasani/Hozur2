namespace FrontClientBBB_Blazor.MiddleWare
{
    using Microsoft.AspNetCore.Mvc.Filters;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireT2CookieAttribute : Attribute
    {
        public string ExpectedValue { get; }

        public RequireT2CookieAttribute(string expectedValue)
        {
            ExpectedValue = expectedValue;
        }
    }

    public class T2CookieMiddleware
    {
        private readonly RequestDelegate _next;

        public T2CookieMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var attr = endpoint?.Metadata.GetMetadata<RequireT2CookieAttribute>();

            if (attr != null)
            {
                if (!context.Request.Cookies.TryGetValue("t2", out var value) ||
                    value != attr.ExpectedValue)
                {
                    context.Response.Redirect("/login");
                    return;
                }
            }

            await _next(context);
        }
    }

}
