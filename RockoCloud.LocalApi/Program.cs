using Microsoft.Extensions.FileProviders;
using RockoCloud.BusinessLogic;
using RockoCloud.BusinessLogic.Interfaces;
using RockoCloud.DataAccess;
using RockoCloud.DataAccess.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RockoCloud");
Directory.CreateDirectory(dbPath);
var connectionString = $"Data Source={Path.Combine(dbPath, "rockola.db")}";

builder.Services.AddSingleton<IDbConnectionFactory>(new DbConnectionFactory(connectionString));
builder.Services.AddScoped<ISongRepository, SongRepository>();
builder.Services.AddScoped<IMusicScannerService, MusicScannerService>();
builder.Services.AddScoped<IFileManagerService, FileManagerService>();
builder.Services.AddScoped<IDownloadService, DownloadService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.MapControllers();

app.Run();