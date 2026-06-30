using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ServiceApp.API.Middleware;
using ServiceApp.Application;
using ServiceApp.Application.Interfaces;
using ServiceApp.Infrastructure;
using ServiceApp.Infrastructure.Persistence;
using ServiceApp.Infrastructure.Settings;

var builder = WebApplication.CreateBuilder(args);

// ---- Application & Infrastructure layers ----
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ---- MVC controllers (enums serialized as strings) ----
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// ---- JWT authentication ----
var jwt = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
if (jwt is null || string.IsNullOrWhiteSpace(jwt.SecretKey) || jwt.SecretKey.Length < 32)
    throw new InvalidOperationException(
        "Jwt configuration is missing or the SecretKey is shorter than 32 characters.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// ---- Health checks (liveness probe for the cloud platform) ----
builder.Services.AddHealthChecks();

// ---- Swagger with JWT bearer support ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ServiceApp API", Version = "v1" });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste your JWT access token (without the 'Bearer ' prefix).",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", scheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });
});

var app = builder.Build();

// ---- Apply migrations & seed baseline data ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // SQL Server (relational) runs EF migrations; the in-memory provider used by integration
    // tests isn't relational, so just materialize the schema graph instead.
    if (db.Database.IsRelational())
        await db.Database.MigrateAsync();
    else
        await db.Database.EnsureCreatedAsync();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await DbSeeder.SeedAsync(db, hasher);
}

// ---- Pipeline ----
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Behind a reverse proxy (Azure App Service terminates TLS), honor the original scheme/host
// so UseHttpsRedirection sees the real https request and generated links are correct.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Anonymous liveness endpoint for load balancers / App Service health probes.
app.MapHealthChecks("/health");

app.Run();

// Exposed so WebApplicationFactory<Program> can bootstrap the app in integration tests.
public partial class Program { }
