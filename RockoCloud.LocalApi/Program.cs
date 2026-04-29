using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RockoCloud.BusinessLogic;
using RockoCloud.BusinessLogic.Interfaces;
using RockoCloud.DataAccess;
using RockoCloud.DataAccess.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RockoCloud");
Directory.CreateDirectory(dbPath);
var connectionString = $"Data Source={Path.Combine(dbPath, "rockola.db")}";

builder.Services.AddSingleton<IDbConnectionFactory>(new DbConnectionFactory(connectionString));

var adminDbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=RockoCloud_AdminDB;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(adminDbConnectionString));

// LA LLAVE AHORA SE LEE DIRECTO DEL APPSETTINGS
var jwtKey = builder.Configuration["Jwt:Key"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"], // SE LEE DEL APPSETTINGS
            ValidAudience = builder.Configuration["Jwt:Audience"], // SE LEE DEL APPSETTINGS
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<ISongRepository, SongRepository>();
builder.Services.AddScoped<IMusicScannerService, MusicScannerService>();
builder.Services.AddScoped<IFileManagerService, FileManagerService>();
builder.Services.AddScoped<IDownloadService, DownloadService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RockoCloud API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Ingresa el token JWT así: Bearer {tu_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options => {
    options.AddPolicy("AllowTauri", policy => {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

var factory = app.Services.GetRequiredService<IDbConnectionFactory>();
DbInitializer.Initialize(factory);

app.UseSwagger();
app.UseSwaggerUI();

var musicPath = "C:\\RockoCloud_Music";
if (!Directory.Exists(musicPath)) Directory.CreateDirectory(musicPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(musicPath),
    RequestPath = "/media"
});

app.UseCors("AllowTauri");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();