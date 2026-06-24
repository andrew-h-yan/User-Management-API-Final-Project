using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class TokenAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly HashSet<string> ValidTokens = new(StringComparer.Ordinal) { "token123", "secret-token", "demo-token" };

    public TokenAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Authorization", out var authorizationHeader) ||
            !authorizationHeader.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            await ReturnUnauthorizedAsync(context, "Missing or malformed Authorization header.");
            return;
        }

        var token = authorizationHeader.ToString().Substring("Bearer ".Length).Trim();
        if (string.IsNullOrEmpty(token) || !ValidTokens.Contains(token))
        {
            await ReturnUnauthorizedAsync(context, "Invalid token.");
            return;
        }

        await _next(context);
    }

    private static Task ReturnUnauthorizedAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsJsonAsync(new { Message = message });
    }
}
