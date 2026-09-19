using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

var app = builder.Build();

/*
 * Configuración OpenApi
 * https://learn.microsoft.com/es-es/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-10.0&tabs=visual-studio%2Cvisual-studio-code
 * https://localhost:{PORT}/openapi/v1.json
 * 
 * Configuración Scalar
 * https://scalar.com/products/api-references/integrations/aspnetcore/integration#basic-setup
 */
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
};


// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapGet("/", () => "Hello world!");

app.Run();

