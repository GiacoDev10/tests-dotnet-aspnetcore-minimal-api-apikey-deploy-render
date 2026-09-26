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
                .WithMethods("GET")
                .AllowAnyHeader();
        });
});

builder.Services.AddOpenApi();
builder.Services.AddApiKeySecurity(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
};

if (app.Environment.IsProduction())
{
    app.UseHsts();
};

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);

app.MapGet("/", () => $"Hello world!");

app.MapGet("/secure", () => $"Hello secure world! {secretKey}")
    .RequireAuthorization();

app.Run();

