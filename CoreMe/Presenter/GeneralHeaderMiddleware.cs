using Application.DTO.Generals;

namespace CoreMe.Presenter
{
    public class GeneralHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public GeneralHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IGeneralHeadersService headersService)
        {
            var headers = await headersService.GetHeadersAsync(context);

            // ذخیره در HttpContext.Items برای دسترسی در کنترلرها
            context.Items["GeneralHeaders"] = headers;

            await _next(context);
        }
    }
    public static class HttpContextExtensions
    {
        public static GeneralHeaders GetGeneralHeaders(this HttpContext context)
        {
            return context.Items["GeneralHeaders"] as GeneralHeaders;
        }
    }
}
