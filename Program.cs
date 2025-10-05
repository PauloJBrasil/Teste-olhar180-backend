using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskManager.Api.Data;
using TaskManager.Api.Models;
 using TaskManager.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});

// Rotas versionadas são aplicadas nos Controllers via prefixo /api/v1

// CORS: liberar acesso para qualquer origem, método e cabeçalho
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// EF Core + SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=tasks.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// JWT Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secret = builder.Configuration["Jwt:Secret"] ?? "b6f9d3c2a1e04f8bb3c76a5d9e2f1c0a7b8d6e5f4a3b2c1d0e9f8a7b6c5d4e3f";
        var issuer = builder.Configuration["Jwt:Issuer"] ?? "TaskManager.Api";
        var audience = builder.Configuration["Jwt:Audience"] ?? "TaskManager.Client";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddSingleton<JwtTokenGenerator>();

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Enable HTTPS redirection only if an HTTPS URL is configured
var urlsConfig = app.Configuration["Urls"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? string.Empty;
if (urlsConfig.Contains("https://", StringComparison.OrdinalIgnoreCase))
{
    app.UseHttpsRedirection();
}
app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Ensure DB created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Apply migrations to ensure schema (Users, Tasks) is created/updated
    db.Database.Migrate();
}


app.Run();
