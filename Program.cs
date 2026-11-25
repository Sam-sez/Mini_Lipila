using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MiniLipila.Data;
using MiniLipila.Models;
using MiniLipila.Models.Lipila;
using MiniLipila.Services;
using Npgsql;
using PostgresEnumExtension;

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

            
            settings.AddAuth("Bearer", new()
            {
                Type = NSwag.OpenApiSecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT"
            });
        };
    });

// Read the database connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

// Part 1: tell the database driver about our enums.
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapAllEnums();
var dataSource = dataSourceBuilder.Build();

// Part 2: tell Entity Framework about our enums.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dataSource, npgsql => npgsql.MapAllEnums()));

// Read the "Lipila" section of appsettings into LipilaSettings
builder.Services.Configure<LipilaSettings>(builder.Configuration.GetSection("Lipila"));

// Identity: users, passwords and roles, saved through our existing AppDbContext
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;   // no two accounts with the same email
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// JWT: the server checks the token on every request.
// This comes AFTER AddIdentity, so Bearer replaces Identity's cookie default.
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"]
                  ?? throw new InvalidOperationException("Jwt:Key is not set");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ClockSkew = TimeSpan.Zero,   // expire exactly on time

            // pin the claim types so role checks and User identity never depend on defaults
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier
        };
    });

builder.Services.AddAuthorization();

// Services used by the disbursement and login endpoints
builder.Services.AddScoped<LipilaKycService>();
builder.Services.AddScoped<CustomerResolver>();
builder.Services.AddScoped<JwtTokenService>();

var app = builder.Build();

// Make sure the three roles (and the first admin) exist before the app takes requests
using (var scope = app.Services.CreateScope())
{
    await RoleSeeder.SeedAsync(scope.ServiceProvider);
}

// Enable Swagger UI
app.UseSwaggerGen();

// Who is calling, then what are they allowed to do. This order matters.
app.UseAuthentication();
app.UseAuthorization();

// Enable FastEndpoints. Enums go in and out of the API as text names, like production.
app.UseFastEndpoints(c =>
{
    c.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
});

app.Run();
