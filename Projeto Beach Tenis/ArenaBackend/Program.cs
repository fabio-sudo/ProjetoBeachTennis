using ArenaBackend.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Configure DbContext with SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=DESKTOP-HEJGC8G\\SQLEXPRESS;Database=ArenaManagementDB;Trusted_Connection=True;TrustServerCertificate=True;";

builder.Services.AddDbContext<ArenaDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddScoped<ArenaBackend.Services.IAnalyticsService, ArenaBackend.Services.AnalyticsService>();
builder.Services.AddScoped<ArenaBackend.Services.ITabsService, ArenaBackend.Services.TabsService>();

// Configure CORS for local frontend testing
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Serve Frontend Static Files
var frontendPath = Path.Combine(builder.Environment.ContentRootPath, "..", "ArenaFrontend");
if (Directory.Exists(frontendPath))
{
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = new PhysicalFileProvider(frontendPath),
        DefaultFileNames = new List<string> { "pages/index.html" }
    });
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(frontendPath)
    });
}

// Redirect root URL to index.html (served by static files) or Swagger as fallback
app.MapGet("/", (HttpContext context) =>
{
    if (Directory.Exists(frontendPath))
    {
        var indexPath = Path.Combine(frontendPath, "pages", "index.html");
        if (File.Exists(indexPath)) return Results.Redirect("/pages/index.html");
    }
    return Results.Redirect("/swagger");
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

app.Run();