namespace GovUk.Education.ExploreEducationStatistics.Content.Api;

public class SeoSecurityHeaderMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        context.Response.OnStarting(
            state =>
            {
                var httpContext = (HttpContext)state;
                httpContext.Response.Headers.Append("X-Frame-Options", "DENY");
                httpContext.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
                httpContext.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                httpContext.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");

                return Task.CompletedTask;
            },
            context
        );

        await next(context);
    }
}
