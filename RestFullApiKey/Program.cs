using RestFullApiKey.Endpoints;
using RestFullApiKey.Security;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
var secretKey = builder.Configuration["SecretKey"];
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins(allowedOrigins)
                .WithMethods("GET", "POST")
                .AllowAnyHeader()
                .WithExposedHeaders("Retry-After");
        });
});
builder.Services.AddValidation();

builder.Services.AddOpenApi();
builder.Services.AddApiKeySecurity(builder.Configuration);
builder.Services.AddRenderForwardedHeaders();
builder.Services.AddPostRateLimiting(builder.Configuration);
builder.Services.AddTurnstileSecurity(builder.Configuration);
builder.Services.AddHttpClient<TurnstileService>(c => c.Timeout = TimeSpan.FromSeconds(10));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseHttpsRedirection();
}
;

if (app.Environment.IsProduction())
{
    app.UseHsts();
}
;

// Configure the HTTP request pipeline.
app.UseForwardedHeaders();
app.UseCors(MyAllowSpecificOrigins);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => $"Hello world!");

app.MapGet("/secure", () => $"Hello secure world! {secretKey}")
    .RequireAuthorization();

app.MapGet("/userip", (HttpContext ctx) => Results.Ok(new
{
    remote = ctx.Connection.RemoteIpAddress?.ToString(),
    xff = ctx.Request.Headers["X-Forwarded-For"].ToString(),
    xOriginalFor = ctx.Request.Headers["X-Original-For"].ToString(),
    cfConnectingIp = ctx.Request.Headers["CF-Connecting-IP"].ToString(),
    trueClientIp = ctx.Request.Headers["True-Client-IP"].ToString()
}))
    .RequireAuthorization();

app.MapDataEndpoints();

app.Run();

