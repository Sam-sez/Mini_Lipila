using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using MiniLipila.Data;

var builder = WebApplication.CreateBuilder(args);

// Register FastEndpoints and Swagger
builder.Services
    .AddFastEndpoints()
    .SwaggerDocument(options =>
    {
        options.DocumentSettings = settings =>
        {
            settings.Title = "MiniLipila API";
            settings.Version = "v1";
            settings.Description = "MiniLipila API Documentation";
        };
    });

// Register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

var app = builder.Build();

// Enable Swagger UI
app.UseSwaggerGen();

// Enable FastEndpoints
app.UseFastEndpoints();

app.Run();
