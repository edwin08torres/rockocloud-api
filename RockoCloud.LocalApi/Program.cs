using RockoCloud.DataAccess;
using RockoCloud.DataAccess.Interfaces;
using RockoCloud.BusinessLogic;
using RockoCloud.BusinessLogic.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de SQLite
var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RockoCloud");
Directory.CreateDirectory(dbPath);
var connectionString = $"Data Source={Path.Combine(dbPath, "rockola.db")}";

// 2. Inyección de Dependencias
builder.Services.AddSingleton<IDbConnectionFactory>(new DbConnectionFactory(connectionString));
builder.Services.AddScoped<ISongRepository, SongRepository>();
builder.Services.AddScoped<IMusicScannerService, MusicScannerService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. CORS para que React/Tauri pueda conectar
builder.Services.AddCors(options => {
    options.AddPolicy("AllowTauri", policy => {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// 4. Inicializar DB al arrancar
var factory = app.Services.GetRequiredService<IDbConnectionFactory>();
DbInitializer.Initialize(factory);

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowTauri");
app.MapControllers();

app.Run();