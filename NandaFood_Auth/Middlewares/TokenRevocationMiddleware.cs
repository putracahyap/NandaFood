using System.Net;
using NandaFood_Auth.Data;
using NandaFood_Auth.Models.Global;

namespace NandaFood_Auth.Middlewares;

public class TokenRevocationMiddleware : IMiddleware
{
    private readonly ILogger<TokenRevocationMiddleware> _logger;
    private readonly NandafoodContext _dbContext;

    public TokenRevocationMiddleware(ILogger<TokenRevocationMiddleware> logger, NandafoodContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/api/authentication/login") ||
            context.Request.Path.StartsWithSegments("/api/authentication/register"))
        {
            await next(context);
            return;
        }
        
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        
        if (authHeader != null)
        {
            try
            {
                var token = authHeader["Bearer ".Length..].Trim();
                
                // Check if the token is revoked
                if (IsTokenRevoked(token))
                {
                    await WriteUnauthorizedResponse(context, "Token has been revoked.");
                    return;
                }

                await next(context); // Call the next middleware in the pipeline
            }
            catch (Exception ex)
            {
                _logger.LogError($"Token validation failed: {ex.Message}");
                await WriteUnauthorizedResponse(context, "Invalid token.");
            }
        }
        else
        {
            await context.Response.WriteAsync("Authorization token is missing.");
        }
    }
    
    private bool IsTokenRevoked(string tokenId)
    {
        // Delegate to your revocation service to check if token is revoked
        return _dbContext.RevokedTokens.Any(rt => rt.Token == tokenId);
    }
    
    private async Task WriteUnauthorizedResponse(HttpContext context, string message)
    {
        var response = new ApiMessage<object>
        {
            StatusCode = (int)HttpStatusCode.Unauthorized,
            Status = "Unauthorized",
            Message = message
        };

        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.ContentType = "application/json";
        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(jsonResponse);
    }
}
