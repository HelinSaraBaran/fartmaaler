using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using FartmaalerAPI.Repositories.Interfaces;
using FartmaalerAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
// Halloooooo Heeeey WOOOOOHOOOOO

var builder = WebApplication.CreateBuilder(args);

// Database forbindelse
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Tilføjer controllers og Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Indtast dit JWT token"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS policy til frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Tilføjer repositories
builder.Services.AddScoped<IRepository<Group>, GroupsRepo>();
builder.Services.AddScoped<IRepository<Session>, SessionsRepo>();
builder.Services.AddScoped<IRepository<Measurement>, MeasurementsRepo>();
builder.Services.AddScoped<MeasurementsRepo>();

// Tilføjer services
builder.Services.AddScoped<MeasurementService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<LeaderboardService>();

var app = builder.Build();

// Seeder standard data
using (IServiceScope scope = app.Services.CreateScope())
{
    AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Seeder admin bruger
    if (!context.Users.Any())
    {
        context.Users.Add(new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "admin"
        });

        context.SaveChanges();
    }

    // Seeder standard grupper
    if (!context.Groups.Any())
    {
        context.Groups.AddRange(
            new Group
            {
                Name = "Gruppe 1",
                School = "Roskilde Skole",
                IsLocked = false
            },
            new Group
            {
                Name = "Gruppe 2",
                School = "Roskilde Skole",
                IsLocked = false
            }
        );

        context.SaveChanges();
    }

    //  Roskilde Skole
    List<Group> oldGroups = context.Groups
        .Where(group => group.School == "Køge Skole")
        .ToList();

    for (int index = 0; index < oldGroups.Count; index++)
    {
        oldGroups[index].School = "Roskilde Skole";
    }

    if (oldGroups.Count > 0)
    {
        context.SaveChanges();
    }

    // Seeder mock skole leaderboard data
    if (!context.SchoolLeaderboardMocks.Any())
    {
        context.SchoolLeaderboardMocks.AddRange(
            new SchoolLeaderboardMock
            {
                SchoolName = "Roskilde Skole",
                RoadType = "byzone 50",
                AverageScore = 8.4,
                AverageCo2 = 5.2,
                MeasurementCount = 42
            },
            new SchoolLeaderboardMock
            {
                SchoolName = "Holbæk Skole",
                RoadType = "byzone 50",
                AverageScore = 10.1,
                AverageCo2 = 6.0,
                MeasurementCount = 38
            },
            new SchoolLeaderboardMock
            {
                SchoolName = "Roskilde Skole",
                RoadType = "landevej 80",
                AverageScore = 12.5,
                AverageCo2 = 8.8,
                MeasurementCount = 35
            },
            new SchoolLeaderboardMock
            {
                SchoolName = "Holbæk Skole",
                RoadType = "landevej 80",
                AverageScore = 14.3,
                AverageCo2 = 9.5,
                MeasurementCount = 31
            },
            new SchoolLeaderboardMock
            {
                SchoolName = "Roskilde Skole",
                RoadType = "motorvej 110",
                AverageScore = 18.3,
                AverageCo2 = 14.1,
                MeasurementCount = 29
            },
            new SchoolLeaderboardMock
            {
                SchoolName = "Holbæk Skole",
                RoadType = "motorvej 110",
                AverageScore = 20.6,
                AverageCo2 = 15.7,
                MeasurementCount = 25
            }
        );

        context.SaveChanges();
    }

    // Seeder globale settings
    List<Settings> requiredSettings = new List<Settings>
    {
        new Settings { Key = "TTS", Value = true },
        new Settings { Key = "BipLyd", Value = true },
        new Settings { Key = "FunFacts", Value = true },
        new Settings { Key = "TTSFunFact", Value = true },
        new Settings { Key = "Leaderboard", Value = true },
        new Settings { Key = "VisuelFeedback", Value = true }
    };

    foreach (Settings requiredSetting in requiredSettings)
    {
        bool settingExists = context.Settings
            .Any(setting => setting.Key.ToLower() == requiredSetting.Key.ToLower());

        if (!settingExists)
        {
            context.Settings.Add(requiredSetting);
        }
    }

    context.SaveChanges();
}

// Bruger Swagger ogsaa paa Azure
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// CORS skal ligge for authentication
app.UseCors("AllowFrontend");

// Bruger authentication og authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();