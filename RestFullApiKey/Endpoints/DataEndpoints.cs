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

    private static IResult HandlerData([FromBody] DataDto dto)
    {
        return Results.Ok(dto);
    }
}
