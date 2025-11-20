namespace MiddleWares
{
    public class UserIpMiddleware
{
    private readonly RequestDelegate _next;

    public UserIpMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString();
        context.Items["UserIP"] = ip;

        await _next(context);
    }
}

}
