using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WebCrawler.Data;
using WebCrawler.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. PostgreSQL-Datenbank konfigurieren
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Identity hinzufügen
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// 3. Authentifizierung mit JWT konfigurieren
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// 4. Autorisierung aktivieren
builder.Services.AddAuthorization();

// 5. Swagger + Token Support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WebCrawler API",
        Version = "v1",
        Description = "Swagger documentation for WebCrawler API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT mit 'Bearer ' prefix. Beispiel: Bearer eyJhbGciOiJIUzI1NiIs...",
        Name = "Authorization",
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

// 6. Controller & statische Dateien
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.WebHost.UseKestrel().UseUrls("http://0.0.0.0:80");

var app = builder.Build();
app.UseDeveloperExceptionPage(); // direkt nach builder.Build()

// 7. HTTP-Pipeline Middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebCrawler API v1");
    c.RoutePrefix = "swagger";
});

app.UseStaticFiles();      // wwwroot/index.html usw.
app.UseAuthentication();   // <- WICHTIG für JWT
app.UseAuthorization();    // <- erst NACH Authentication

app.MapControllers();
app.Run();
