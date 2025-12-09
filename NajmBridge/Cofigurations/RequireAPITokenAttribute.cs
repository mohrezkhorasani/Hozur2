using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NajmBridge.Cofigurations
{
    
   
    public class RequireApiTokenAttribute : ActionFilterAttribute
    {
        private const string ApiToken = "X7kP9mN2vL8qW3eR5tY6uI4oP9aS2dF1gH3jK5lM7nQ8rT9vU0wX2yZ4";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var hasHeader = context.HttpContext.Request.Headers.TryGetValue("api-token", out var token);

            if (!hasHeader || token != ApiToken)
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
