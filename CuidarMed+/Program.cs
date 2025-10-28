using Application.Interfaces;
using Application.Services;
using Infraestructure.Command;
using Infraestructure.Middleware;
using Infraestructure.Persistence;
using Infraestructure.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAuthorization();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Obtenego la cadena de conexin
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    // Si esta parte falla, es la causa del error.
    throw new InvalidOperationException("La cadena de conexin 'DefaultConnection' no fue encontrada en appsettings.json.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString) // Pasa la cadena LEDA aqu
);

// ========== QUERIES (lectura) - Infrastructure ==========
builder.Services.AddScoped<IDoctorQuery, DoctorQuery>();

// ========== COMMANDS (escritura) - Infrastructure ==========
builder.Services.AddScoped<IDoctorCommand, DoctorCommand>();

// ========== SERVICES - Doctors ==========
builder.Services.AddScoped<ICreateDoctorService, CreateDoctorService>();
builder.Services.AddScoped<ISearchDoctorService, SearchDoctorService>();
builder.Services.AddScoped<IUpdateDoctorService, UpdateDoctorService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<JwtValidationMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
