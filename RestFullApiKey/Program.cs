using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
var secretKey = builder.Configuration["SecretKey"];

// Add services to the container.
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
};


// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapGet("/", () => $"Hello world! {secretKey}");

app.Run();

