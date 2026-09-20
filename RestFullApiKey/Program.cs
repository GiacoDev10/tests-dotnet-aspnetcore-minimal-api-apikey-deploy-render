using RestFullApiKey.Options;
using RestFullApiKey.Security;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
var secretKey = builder.Configuration["SecretKey"];

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services
    .AddAuthentication(ApiKeyAuthenticationOptions.DefaultScheme)
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme,
    options =>
    {
        options.SecretKey = secretKey;
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
};


// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapGet("/", () => $"Hello world!");

app.MapGet("/secure", () => $"Hello secure world! {secretKey}")
    .RequireAuthorization();

app.Run();

