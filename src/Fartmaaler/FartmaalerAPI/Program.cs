using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using FartmaalerAPI.Repositories.Interfaces;
using FartmaalerAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
        Type = Microsoft.OpenApi.Models.ParameterLocation.Header,
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
    AppDbContext context =
        scope.ServiceProvider.GetRequiredService<AppDbContext>();

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

    // Retter gamle grupper fra Køge Skole til Roskilde Skole
    List<Group> oldGroups =
        context.Groups
            .Where(group => group.School == "Køge Skole")
            .ToList();

    for (int index = 0; index < oldGroups.Count; index++)
    {
        oldGroups[index].School =
            "Roskilde Skole";
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
    List<Settings> requiredSettings =
        new List<Settings>
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
        bool settingExists =
            context.Settings.Any(
                setting =>
                    setting.Key.ToLower() ==
                    requiredSetting.Key.ToLower()
            );

        if (!settingExists)
        {
            context.Settings.Add(requiredSetting);
        }
    }

    context.SaveChanges();

    // Seeder fun facts til database
    if (!context.FunFacts.Any())
    {
        context.FunFacts.AddRange(
            new FunFact
            {
                Text = "Cykling og gang udleder 0 gram CO₂ under selve turen. Hvis du cykler i stedet for at tage bilen på korte ture, sparer du derfor bilens direkte udledning."
            },
            new FunFact
            {
                Text = "Transport står for cirka en fjerdedel af EU’s samlede drivhusgasudledning. Derfor betyder vores transportvaner meget for klimaet."
            },
            new FunFact
            {
                Text = "Hvis du vælger cykel eller gang én dag om ugen i stedet for bilen, kan det være med til at reducere din transportudledning over tid."
            },
            new FunFact
            {
                Text = "Jævn kørsel kan bruge mindre energi end hårde accelerationer og pludselige opbremsninger. Derfor handler bæredygtig kørsel også om kørestil."
            },
            new FunFact
            {
                Text = "Jo hurtigere en bil kører, jo mere energi skal den bruge på at overvinde luftmodstand. Derfor kan meget høj fart øge energiforbruget."
            },
            new FunFact
            {
                Text = "Hvis en biltur udleder omkring 150 gram CO₂ pr. kilometer som et simpelt estimat, vil en cykeltur på 5 km spare omkring 750 gram CO₂ sammenlignet med bilen."
            },
            new FunFact
            {
                Text = "Hvis du undgår 10 korte bilture på 5 km, og bilen udleder cirka 150 gram CO₂ pr. kilometer, svarer det til omkring 7,5 kg CO₂ sparet."
            },
            new FunFact
            {
                Text = "Hvis du sparer penge ved at tage bilen færre gange, kan de penge over tid bruges på noget andet. En dyr telefon som en iPhone Pro Max kan koste omkring 10.000-14.000 kr. afhængigt af model og lagerplads."
            },
            new FunFact
            {
                Text = "Små valg tæller: én kort biltur virker lille, men mange korte ture hver uge kan blive til meget CO₂ over et helt år."
            },
            new FunFact
            {
                Text = "Bæredygtig transport handler ikke kun om at køre langsomt. Det handler også om at vælge det rigtige transportmiddel til turen."
            }
        );

        context.SaveChanges();
    }
}

// Bruger Swagger også på Azure
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// CORS skal ligge før authentication
app.UseCors("AllowFrontend");

// Bruger authentication og authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();