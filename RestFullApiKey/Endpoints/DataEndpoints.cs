using Microsoft.AspNetCore.Mvc;
using RestFullApiKey.Data;
using RestFullApiKey.Security;

namespace RestFullApiKey.Endpoints;

public static class DataEndpoints
{
    public static void MapDataEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/data", HandlerData)
            .RequireRateLimiting(RateLimitingExtensions.PostPolicyName)
            .RequireAuthorization();
    }

    private async static Task<IResult> HandlerData([FromBody] DataDto dto, TurnstileService service, HttpContext ctx)
    {

        var remoteip = ctx.Request.Headers["CF-Connecting-IP"].FirstOrDefault()
            ?? ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? ctx.Connection.RemoteIpAddress?.ToString();

        if (string.IsNullOrEmpty(remoteip))
        {
            return Results.InternalServerError();
        }

        var response = await service.ValidateTokenAsync(dto.TurnstileToken, remoteip);

        if (!response.Success)
        {
            return Results.BadRequest<TurnstileResponse>(response);
        }

        return Results.Ok<TurnstileResponse>(response);
    }
}
