using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using WebCrawler.Data;
using WebCrawler.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel().UseUrls("http://0.0.0.0:80");

builder.Services.AddControllers();

// Register EF Core with PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WebCrawler API",
        Version = "v1",
        Description = "Swagger documentation for WebCrawler API"
    });
});

var app = builder.Build();

// Enable Swagger always
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebCrawler API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthorization();
app.MapControllers();


app.Run();
