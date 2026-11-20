using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MiniLipila.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseFastEndpoints();

app.Run();