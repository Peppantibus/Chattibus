using System.Text;

namespace Chat.Utilities;
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Rimuove info server
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");

        // X-Frame-Options (legacy)
        context.Response.Headers["X-Frame-Options"] = "DENY";
        // No MIME sniffing
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        // No referrer leakage
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
        // Permissions policy
        context.Response.Headers["Permissions-Policy"] =
            "geolocation=(), microphone=(), camera=(), payment=(), usb=()";
        //ulteriore sicurezza per xss su browser vecchi
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

        // CSP per API
        var csp = new StringBuilder()
            .Append("default-src 'none'; ")
            .Append("connect-src 'self'; ")
            .Append("img-src 'none'; ")
            .Append("object-src 'none'; ")
            .Append("frame-ancestors 'none'; ")
            .Append("base-uri 'none'; ")
            .Append("form-action 'none';");
        context.Response.Headers["Content-Security-Policy"] = csp.ToString();

        // Strict Transport Security
        if (context.Request.IsHttps)
        {
            context.Response.Headers["Strict-Transport-Security"] =
                "max-age=31536000; includeSubDomains; preload"; 
        }

        // Cache-Control
        context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate"; 
        context.Response.Headers["Pragma"] = "no-cache";

        await _next(context);
    }
}

public static class SecurityHeadersExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}