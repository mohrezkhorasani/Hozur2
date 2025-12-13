using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NajmBridge.Cofigurations
{
    public class RequireApiTokenAttribute : ActionFilterAttribute
    {
        private const string ApiToken = "X7kP9mN2vL8qW3eR5tY6uI4oP9aS2dF1gH3jK5lM7nQ8rT9vU0wX2yZ4";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;
            var host = request.Host.Host; // فقط نام هاست بدون پورت
            var scheme = request.Scheme;  // http یا https

            // شرط ۱: دامنه دقیق hekmat-najmapi.piepa.ir
            bool isNajmApi = string.Equals(host, "hekmat-najmapi.piepa.ir", StringComparison.OrdinalIgnoreCase);

            // شرط ۲: هر زیر دامنه از hekmateislami.com
            bool isHekmatSubdomain = host.EndsWith(".hekmateislami.com", StringComparison.OrdinalIgnoreCase)
                                     && host.Length > "hekmateislami.com".Length; // برای جلوگیری از تطابق دقیق hekmateislami.com

            // اگر یکی از دو شرط بالا درست بود → اجازه بده بدون چک توکن
            if (isNajmApi || isHekmatSubdomain)
            {
                base.OnActionExecuting(context);
                return;
            }

            // در غیر این صورت: چک کردن هدر api-token
            if (!request.Headers.TryGetValue("api-token", out var token) || token != ApiToken)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    status = 401,
                    message = "دسترسی غیرمجاز. توکن API معتبر نیست یا وجود ندارد."
                });
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}