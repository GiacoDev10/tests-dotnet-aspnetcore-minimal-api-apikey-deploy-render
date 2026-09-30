using Microsoft.AspNetCore.Mvc;
using RestFullApiKey.Data;

namespace RestFullApiKey.Endpoints;

public static class DataEndpoints
{
    public static void MapDataEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/data", HandlerData)
            .RequireAuthorization();
    }

    private static IResult HandlerData([FromBody] DataDto dto)
    {
        return Results.Ok(dto.message);
    }
}
